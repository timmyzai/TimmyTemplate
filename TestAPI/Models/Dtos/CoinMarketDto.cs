using AwesomeProject;

namespace TestAPI.Models.Dtos
{
    public class CoinMarketDto : EntityDto<string>
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public decimal PriceUsd { get; set; }
        public decimal PriceMyr { get; set; }

    }
}