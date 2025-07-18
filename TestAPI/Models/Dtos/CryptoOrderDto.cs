using AwesomeProject;

namespace TestAPI.Models.Dtos
{
    public class CryptoOrderDto : FullyAuditedEntityDto<Guid>
    {
        public Guid UserId { get; set; }
        public string CoinId { get; set; }
        public double UsdAmount { get; set; }
        public double CoinAmount { get; set; }
        public double PriceUsd { get; set; }
        public OrderType Type { get; set; }
    }
}