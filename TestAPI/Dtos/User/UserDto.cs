using AwesomeProject;

namespace TestAPI.Dtos.User;

public class UserDto : EntityDto<Guid>
{
    public string Username { get; set; }
    public string EmailAddress { get; set; }
    public string PhoneNumber { get; set; }
    public string CountryName { get; set; }
    public string RoleNames { get; set; }
}