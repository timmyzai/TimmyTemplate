using System.Text.Json.Serialization;
using TestAPI.Dtos.ExchangeRate;
using TestAPI.Helper.StaticClasses;

namespace TestAPI.External.ExchangeRateAPI;

public class ExchangeRateResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("base")]
    public string BaseCurrency { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("rates")]
    [JsonConverter(typeof(JsonHelper.RatesJsonConverter))]
    public IEnumerable<ExchangeRateDto> Rates { get; set; } = new List<ExchangeRateDto>();
}