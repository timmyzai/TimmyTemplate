using System.Text.Json.Serialization;

namespace ByteAwesome.TestAPI.Data;

public class CountryCurrency
{
    [JsonPropertyName("Country")]
    public string CountryName { get; set; }
    
    [JsonPropertyName("Country Code")]
    public string CountryCode_2 { get; set; }
    
    [JsonPropertyName("Currency Code")]
    public string CurrencyCode { get; set; }
}