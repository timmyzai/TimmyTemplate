using System.Transactions;
using ByteAwesome.UserAPI.DbContexts;

namespace ByteAwesome.UserAPI.Helper.UnitOfWork
{
    public interface IUnitOfWorkManager
    {
        ICurrentUnitOfWork Begin();
        ICurrentUnitOfWork Begin(TransactionScopeOption option);
    }
    public class UnitOfWorkManager : IUnitOfWorkManager
    {
        private readonly ApplicationDbContext _dbContext;
        public UnitOfWorkManager(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public ICurrentUnitOfWork Begin()
        {
            return new CurrentUnitOfWork(_dbContext);
        }
        public ICurrentUnitOfWork Begin(TransactionScopeOption option)
        {
            return new CurrentUnitOfWork(_dbContext, option);
        }
    }
}
