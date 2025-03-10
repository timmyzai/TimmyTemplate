using AutoMapper;
using TestAPI.DbContexts;
using TestAPI.Dtos.Wallet;
using TestAPI.Models;
using Microsoft.EntityFrameworkCore;
using AwesomeProject;

namespace TestAPI.Repositories;

public interface IWalletRepository : IBaseRepository<WalletDto,CreateWalletDto,Guid>
{
    public Task<WalletDto> GetByUserId(Guid userId);
};

public class WalletRepository : BaseRepository<Wallet,WalletDto,CreateWalletDto,Guid>, IWalletRepository
{
    private readonly ApplicationDbContext _context;
    
    public WalletRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
        _context = context;
    }

    public async Task<WalletDto> GetByUserId(Guid userId)
    {
        var item = await _context.Wallet.FirstOrDefaultAsync(r => Equals(r.UserId, userId));
        if (item is null)
        {
            return default;
        }
        return MapEntityToDto(item);
    }
}
