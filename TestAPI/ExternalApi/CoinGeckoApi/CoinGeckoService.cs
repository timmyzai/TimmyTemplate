using AwesomeProject;
using TestAPI.Models.Dtos;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace TestAPI.ExternalApi.CoinGeckoApi
{
    public class CoinGeckoService : ICoinGeckoService
    {
        private readonly HttpClient _client;


        public CoinGeckoService(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<CoinsDto>> GetCoinListAsync()
        {
            var url = "/api/v3/coins/list";
            var request = HttpClientHelper.CreateRequestMessage(HttpMethod.Get, url);
            var coinGeckoCoins = await HttpClientHelper.SendRequestAsync<List<CoinGeckoCoin>>(_client, request);

            var coinList = coinGeckoCoins?.Select(x => new CoinsDto
            {
                Id = x.Id, 
                Symbol = x.Symbol,
                Name = x.Name
            }).ToList();

            return coinList;

        }

        public async Task<CoinMarketDto> GetCoinDataByIdAsync(string coinId)
        {
            var url = $"/api/v3/coins/{coinId}?market_data=true";
            var request = HttpClientHelper.CreateRequestMessage(HttpMethod.Get, url);

            var jsonObj = await HttpClientHelper.SendRequestAsync<JObject>(_client, request);

            if (jsonObj == null)
                return null;

            return new CoinMarketDto
            {
                Id = jsonObj["id"]?.ToString(),
                Symbol = jsonObj["symbol"]?.ToString(),
                Name = jsonObj["name"]?.ToString(),
                PriceUsd = jsonObj["market_data"]?["current_price"]?["usd"]?.Value<decimal>() ?? 0,
                PriceMyr = jsonObj["market_data"]?["current_price"]?["myr"]?.Value<decimal>() ?? 0
            };
        }

        public class CoinGeckoCoin
        {
            public string Id { get; set; }
            public string Symbol { get; set; }
            public string Name { get; set; }
        }
    }
}