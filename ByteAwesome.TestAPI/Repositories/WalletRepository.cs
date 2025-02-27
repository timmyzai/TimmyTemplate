using AutoMapper;
using ByteAwesome.TestAPI.DbContexts;
using ByteAwesome.TestAPI.Dtos.Wallet;
using ByteAwesome.TestAPI.Models;

namespace ByteAwesome.TestAPI.Repositories;

public interface IWalletRepository : IBaseRepository<WalletDto,CreateWalletDto,Guid>
{
};

public class WalletRepository : BaseRepository<Wallet,WalletDto,CreateWalletDto,Guid>, IWalletRepository
{
    public WalletRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}
