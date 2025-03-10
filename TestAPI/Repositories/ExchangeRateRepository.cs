using AutoMapper;
using TestAPI.DbContexts;
using TestAPI.Dtos.ExchangeRate;
using TestAPI.Models;

using AwesomeProject;

namespace TestAPI.Repositories;

public interface IExchangeRateRepository : IBaseRepository<ExchangeRateDto, CreateExchangeRateDto, Guid>
{
};

public class ExchangeRateRepository : BaseRepository<ExchangeRate, ExchangeRateDto, CreateExchangeRateDto, Guid>, IExchangeRateRepository
{

    public ExchangeRateRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}