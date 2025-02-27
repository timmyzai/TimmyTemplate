namespace ByteAwesome.TestAPI.Dtos.ExchangeRate;

public class ExchangeRateDto : EntityDto<Guid>
{
    public string TargetCurrency { get; set; }
    public decimal Rate { get; set; }
    public Guid BaseCurrencyId { get; set; }
}