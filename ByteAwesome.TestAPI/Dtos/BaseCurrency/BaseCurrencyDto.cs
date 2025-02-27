using ByteAwesome.TestAPI.Dtos.ExchangeRate;

namespace ByteAwesome.TestAPI.Dtos.BaseCurrency;

public class BaseCurrencyDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public List<ExchangeRateDto> Rates { get; set; }
}