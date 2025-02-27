using AutoMapper;
using ByteAwesome.TestAPI.DbContexts;
using ByteAwesome.TestAPI.Dtos.ExchangeRate;
using ByteAwesome.TestAPI.Models;

namespace ByteAwesome.TestAPI.Repositories;

public interface IExchangeRateRepository : IBaseRepository<ExchangeRateDto, CreateExchangeRateDto, Guid>
{
};

public class ExchangeRateRepository : BaseRepository<ExchangeRate, ExchangeRateDto, CreateExchangeRateDto, Guid>, IExchangeRateRepository
{

    public ExchangeRateRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}