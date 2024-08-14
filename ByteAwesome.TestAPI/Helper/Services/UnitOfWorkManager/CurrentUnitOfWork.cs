using System.Transactions;
using ByteAwesome.TestAPI.DbContexts;
using Serilog;

namespace ByteAwesome.TestAPI.Services
{
    public interface ICurrentUnitOfWork : IDisposable
    {
        Task SaveChangesAsync();
    }

    public class CurrentUnitOfWork : ICurrentUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        private TransactionScope _transactionScope;
        private bool _completed;

        public CurrentUnitOfWork(ApplicationDbContext dbContext, TransactionScopeOption scopeOption = TransactionScopeOption.Required)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.DefaultTimeout
            };
            _transactionScope = new TransactionScope(scopeOption, transactionOptions, TransactionScopeAsyncFlowOption.Enabled);
        }
        public async Task SaveChangesAsync()
        {
            try
            {
                if (_completed)
                {
                    throw new InvalidOperationException("SaveChangesAsync has already been called on this unit of work.");
                }

                await _dbContext.SaveChangesAsync();
                _transactionScope.Complete();
                _completed = true;
                Log.Information("Transaction completed successfully.");
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
            }
        }
        public void Dispose()
        {
            try
            {
                if (!_completed)
                {
                    _transactionScope.Dispose();
                    Log.Information("Transaction rolled back.");
                }
                else
                {
                    _transactionScope.Dispose();
                    Log.Information("Transaction disposed.");
                }
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
            }
        }
    }
}