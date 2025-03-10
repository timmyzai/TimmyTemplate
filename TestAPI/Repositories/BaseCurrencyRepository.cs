using AutoMapper;
using TestAPI.DbContexts;
using TestAPI.Dtos.BaseCurrency;
using TestAPI.Models;
using Microsoft.EntityFrameworkCore;

using AwesomeProject;

namespace TestAPI.Repositories;

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