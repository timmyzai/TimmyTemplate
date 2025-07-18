using AwesomeProject;
using TestAPI.Models;
using TestAPI.Models.Dtos;

namespace TestAPI.Repository
{
  public interface ICryptoOrderRepository : IBaseRepository<CryptoOrderDto, CreateCryptoOrderDto, Guid>
  {

    Task<List<CryptoOrderDto>> GetOrdersByUserIdAsync(Guid userId);

    }
}