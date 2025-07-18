using TestAPI.Models.Dtos;

namespace TestAPI.ExternalApi
{
    public interface ICoinGeckoService
    {
        Task<List<CoinsDto>> GetCoinListAsync();
        Task<CoinMarketDto> GetCoinDataByIdAsync(string coinId);
    }
}