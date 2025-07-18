using AwesomeProject;
using Microsoft.AspNetCore.Mvc;
using TestAPI.ExternalApi;
using TestAPI.Models;
using TestAPI.Models.Dtos;
using TestAPI.Repository;

namespace TestAPI.Service
{
    public class OrderService : IOrderService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICryptoOrderRepository _orderRepository;

        private readonly ICoinGeckoService _coinGeckoService;

        public OrderService(
    IHttpClientFactory httpClientFactory,
    ICryptoOrderRepository orderRepository,
    ICoinGeckoService coinGeckoService
        )

        {
            _httpClientFactory = httpClientFactory;
            _orderRepository = orderRepository;
            _coinGeckoService = coinGeckoService;
        }

        public async Task<CryptoOrderDto> PlaceOrderAsync(CreateCryptoOrderDto input)
        {
            return await _orderRepository.Add(input); 
        }

        public async Task<List<CryptoOrderDto>> GetMyOrdersAsync()
        {
            var userId = AwesomeProject.CurrentSession.GetUserId();
            if (userId == Guid.Empty)
            {
                throw new AppException(ErrorCodes.CryptoOrder.UserIdNotFound);
            }
            return await _orderRepository.GetOrdersByUserIdAsync(userId.Value);
        }

        public async Task<CryptoOrderDto> GetOrderByIdAsync(Guid id)
        {
            return await _orderRepository.GetById(id);
        }

        public async Task<CryptoOrderDto> UpdateOrderAsync([FromBody]UpdateCryptoOrderDto input)
        {
            var existingOrder = await _orderRepository.GetById(input.id);
            if (existingOrder == null)
                throw new AppException(ErrorCodes.CryptoOrder.OrderNotFound);

            // Step 1: Get coin ID
            var coinId = existingOrder.CoinId;

            // Step 2: Fetch live price from CoinGecko
            var coinData = await _coinGeckoService.GetCoinDataByIdAsync(coinId);
            var priceUsd = coinData?.PriceUsd ?? 0;
            if (priceUsd <= 0)
                throw new AppException(ErrorCodes.CryptoOrder.InvalidLivePrice);

            var updated = new CryptoOrderDto
            {
                Id = input.id,
                UserId = existingOrder.UserId,
                CoinId = existingOrder.CoinId,
                CoinAmount = input.UsdAmount / (double)priceUsd,
                PriceUsd = (double)priceUsd,
                UsdAmount = input.UsdAmount,
                Type = input.Type
            };

            return await _orderRepository.Update(updated);
        }

        public async Task<bool> DeleteOrderAsync(Guid id)
        {
            var result = await _orderRepository.Delete(id);

            return true;
        }

    }
}