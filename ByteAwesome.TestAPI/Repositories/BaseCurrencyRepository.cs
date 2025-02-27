using AutoMapper;
using ByteAwesome.TestAPI.DbContexts;
using ByteAwesome.TestAPI.Dtos.BaseCurrency;
using ByteAwesome.TestAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ByteAwesome.TestAPI.Repositories;

public interface IBaseCurrencyRepository : IBaseRepository<BaseCurrencyDto, CreateBaseCurrencyDto, Guid>
{
    Task<BaseCurrencyDto> GetByCurrencyName(string code);
};

public class BaseCurrencyRepository : BaseRepository<BaseCurrency, BaseCurrencyDto, CreateBaseCurrencyDto, Guid>, IBaseCurrencyRepository
{
    public BaseCurrencyRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }

    public async Task<BaseCurrencyDto> GetByCurrencyName(string baseCurrency)
    {
        var existingRate = await ContextEntity<BaseCurrency>()
            .FirstOrDefaultAsync(x => x.Name == baseCurrency);
        
        if (existingRate is null)
        {
            return default;
        }

        return MapEntityToDto(existingRate);
    }
}