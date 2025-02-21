using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ByteAwesome.UnitOfWork
{
    public interface IUnitOfWorkManager
    {
        ICurrentUnitOfWork Begin<TDbContext>() where TDbContext : DbContext;
        ICurrentUnitOfWork Begin<TDbContext>(TransactionScopeOption option) where TDbContext : DbContext;
    }

    public class UnitOfWorkManager : IUnitOfWorkManager
    {
        private readonly IServiceProvider _serviceProvider;

        public UnitOfWorkManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ICurrentUnitOfWork Begin<TDbContext>() where TDbContext : DbContext
        {
            var dbContextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<TDbContext>>();
            var dbContext = dbContextFactory.CreateDbContext();
            return new CurrentUnitOfWork<TDbContext>(dbContext);
        }

        public ICurrentUnitOfWork Begin<TDbContext>(TransactionScopeOption option) where TDbContext : DbContext
        {
            var dbContextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<TDbContext>>();
            var dbContext = dbContextFactory.CreateDbContext();
            return new CurrentUnitOfWork<TDbContext>(dbContext, option);
        }
    }
}
