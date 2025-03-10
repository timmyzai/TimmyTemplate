using AwesomeProject;

namespace TestAPI.Models;

public class BaseCurrency : Entity<Guid>
{
    public string Name { get; set; }
    public ICollection<ExchangeRate> Rates { get; set; }
}