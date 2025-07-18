using AwesomeProject;

namespace TestAPI.Models
{
    public class Coins : Entity<Guid>
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
    }

}