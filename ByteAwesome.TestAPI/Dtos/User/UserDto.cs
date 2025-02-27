namespace ByteAwesome.TestAPI.Dtos.User;

public class UserDto : EntityDto<Guid>
{
    public string Username { get; set; }
    public string CountryName { get; set; }
}