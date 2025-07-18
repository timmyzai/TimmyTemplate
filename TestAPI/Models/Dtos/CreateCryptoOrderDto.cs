using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TestAPI.Models.Dtos
{
    public class CreateCryptoOrderDto
    {

        [Required]
        public string CoinId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public double UsdAmount { get; set; }

        [Required]
        public OrderType Type { get; set; }
        
        [JsonIgnore]
        public Guid UserId { get; set; }
        [JsonIgnore]
        public double CoinAmount { get; set; }

        [JsonIgnore]
        public double PriceUsd { get; set; }



    }

}