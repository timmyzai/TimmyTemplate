using TestAPI.Models;
using TestAPI.Models.Dtos;

namespace TestAPI.Service
{
  public interface IOrderService
  {

    Task<CryptoOrderDto> PlaceOrderAsync(CreateCryptoOrderDto input);

    Task<List<CryptoOrderDto>> GetMyOrdersAsync();

    Task<CryptoOrderDto> GetOrderByIdAsync(Guid id);

    Task<CryptoOrderDto> UpdateOrderAsync(UpdateCryptoOrderDto input);
     
     Task<bool> DeleteOrderAsync(Guid id);


  }
}