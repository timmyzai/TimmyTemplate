using System.Transactions;
using AwesomeProject;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace UnitOfWork
{
    public interface ICurrentUnitOfWork : IDisposable
    {
        Task SaveChangesAsync();
    }

    public class CurrentUnitOfWork<TDbContext> : ICurrentUnitOfWork where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;
        private readonly TransactionScope _transactionScope;
        private bool _completed;

        public CurrentUnitOfWork(TDbContext dbContext, TransactionScopeOption scopeOption = TransactionScopeOption.Required)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.DefaultTimeout
            };
            _transactionScope = new TransactionScope(scopeOption, transactionOptions, TransactionScopeAsyncFlowOption.Enabled);
        }

        public async Task SaveChangesAsync()
        {
            if (_completed)
                throw new InvalidOperationException("SaveChangesAsync has already been called on this unit of work.");

            try
            {
                await _dbContext.SaveChangesAsync();
                _transactionScope.Complete();
                _completed = true;
            }
            catch (Exception ex)
            {
                // Handle or log exception
                throw new InvalidOperationException("An error occurred during the transaction.", ex);
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
