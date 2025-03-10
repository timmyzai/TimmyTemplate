namespace TestAPI.Dtos.User;

public class CreateUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string EmailAddress { get; set; }
    public string PhoneNumber { get; set; }
    public string CountryName { get; set; }
    public string RoleNames { get; set; }
}