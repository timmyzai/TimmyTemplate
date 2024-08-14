using System.Transactions;
using ByteAwesome.UserAPI.DbContexts;
using Serilog;

namespace ByteAwesome.UserAPI.Helper.UnitOfWork
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
            if (_completed)
            {
                throw new InvalidOperationException("SaveChangesAsync has already been called on this unit of work.");
            }

            await _dbContext.SaveChangesAsync();
            _transactionScope.Complete();
            _completed = true;
            Log.Information("Transaction completed successfully.");
        }
        public void Dispose()
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
    }
}