using AwesomeProject;

namespace TestAPI.Models;

public class User : Entity<Guid>
{
    public string Username { get; set; }
    public string CountryName { get; set; }
}