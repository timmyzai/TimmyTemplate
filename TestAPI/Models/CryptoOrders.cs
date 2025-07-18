using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeProject;

namespace TestAPI.Models
{
    public class CryptoOrder : FullyAuditedEntity<Guid>, IUserIdEntity
    {
        public Guid UserId { get; set; }
        public string CoinId { get; set; } // e.g. "bitcoin"
        public double UsdAmount { get; set; }
        public double CoinAmount { get; set; }
        public double PriceUsd { get; set; }
        public OrderType Type { get; set; } // "buy" or "sell"
    }

    [JsonConverter(typeof(OrderTypeFlexibleConverter))]
    public enum OrderType
    {

        Buy = 0,
        Sell = 1

    }

    public class OrderTypeFlexibleConverter : JsonConverter<OrderType>
    {
        public override OrderType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Accept integer values: 0 or 1
            if (reader.TokenType == JsonTokenType.Number)
            {
                var intVal = reader.GetInt32();
                return (OrderType)intVal;
            }

            // Accept string values: "buy", "sell"
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (Enum.TryParse(typeof(OrderType), str, ignoreCase: true, out var result))
                {
                    return (OrderType)result;
                }
            }

            throw new JsonException("Invalid value for OrderType.");
        }

        public override void Write(Utf8JsonWriter writer, OrderType value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((int)value); // Always output as int
        }
    }

}
