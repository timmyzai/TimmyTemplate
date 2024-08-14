using ByteAwesome.UserAPI.Models.Dtos;

namespace ByteAwesome.UserAPI.Repository
{
    public interface IUserRepository : IBaseRepository<UserDto, CreateUserDto, Guid>
    {
        Task<UserDto> GetUserByUserLoginIdentity(string userLoginIdentity);
        Task CheckIfUserExists(string UserName, string PhoneNumber, string EmailAddress);
    }
}