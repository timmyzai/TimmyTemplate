namespace ByteAwesome.TestAPI.Dtos.ExchangeRate;

public class CreateExchangeRateDto
{
    public string TargetCurrency { get; set; }
    public decimal Rate { get; set; }
    public Guid BaseCurrencyId { get; set; }
}