using TestAPI.Dtos.ExchangeRate;

using AwesomeProject;

namespace TestAPI.Dtos.BaseCurrency;

public class BaseCurrencyDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public List<ExchangeRateDto> Rates { get; set; }
}