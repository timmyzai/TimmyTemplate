using AutoMapper;
using AwesomeProject;
using Microsoft.EntityFrameworkCore;
using TestAPI.DbContexts;
using TestAPI.Models;
using TestAPI.Models.Dtos;

namespace TestAPI.Repository
{
    public class CryptoOrderRepository : BaseRepository<CryptoOrder, CryptoOrderDto, CreateCryptoOrderDto, Guid>, ICryptoOrderRepository
    {
        public CryptoOrderRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper) { }

        public async Task<List<CryptoOrderDto>> GetOrdersByUserIdAsync(Guid userId)
        {
            var orders = await ((ApplicationDbContext)context).CryptoOrders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedTime)
                .ToListAsync();

            return mapper.Map<List<CryptoOrderDto>>(orders);
        }

    }
}

