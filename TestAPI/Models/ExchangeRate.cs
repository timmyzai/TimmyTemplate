using System.ComponentModel.DataAnnotations.Schema;

using AwesomeProject;

namespace TestAPI.Models;

public class ExchangeRate : Entity<Guid>
{
    [ForeignKey("BaseCurrencyId")]
    public Guid BaseCurrencyId { get; set; }
    public virtual BaseCurrency BaseCurrency { get; set; }
    public string TargetCurrency { get; set; }
    public decimal Rate { get; set; }
}