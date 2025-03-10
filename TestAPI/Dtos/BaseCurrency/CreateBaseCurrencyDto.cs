using TestAPI.Dtos.ExchangeRate;

namespace TestAPI.Dtos.BaseCurrency;

public class CreateBaseCurrencyDto
{
    public string Name { get; set; }
    public ICollection<ExchangeRateDto> Rates { get; set; }
}