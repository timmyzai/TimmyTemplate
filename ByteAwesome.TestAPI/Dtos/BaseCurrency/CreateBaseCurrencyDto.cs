using ByteAwesome.TestAPI.Dtos.ExchangeRate;

namespace ByteAwesome.TestAPI.Dtos.BaseCurrency;

public class CreateBaseCurrencyDto
{
    public string Name { get; set; }
    public ICollection<ExchangeRateDto> Rates { get; set; }
}