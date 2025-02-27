using System.Text.Json;
using System.Text.Json.Serialization;
using ByteAwesome.TestAPI.Dtos.ExchangeRate;

namespace ByteAwesome.TestAPI.Helper.StaticClasses;

public static class JsonHelper
{
    public class RatesJsonConverter : JsonConverter<IEnumerable<ExchangeRateDto>>
    {
        public override IEnumerable<ExchangeRateDto> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var rates = new List<ExchangeRateDto>();

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Invalid format for rates object.");
            }

            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    property.Value.TryGetDecimal(out decimal rateValue);
                    rates.Add(new ExchangeRateDto()
                    {
                        TargetCurrency = property.Name,
                        Rate = rateValue
                    });
                }
            }

            return rates;
        }

        public override void Write(Utf8JsonWriter writer, IEnumerable<ExchangeRateDto> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
        
            foreach (var rate in value)
            {
                writer.WriteString(rate.TargetCurrency, rate.Rate.ToString());
            }
        
            writer.WriteEndObject();
        }
    }
}