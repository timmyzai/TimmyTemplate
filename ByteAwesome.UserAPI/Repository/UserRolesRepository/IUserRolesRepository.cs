using ByteAwesome.UserAPI.Models.Dtos;

namespace ByteAwesome.UserAPI.Repository
{
    public interface IUserRolesRepository : IBaseRepository<UserRolesDto, CreateUserRolesDto, Guid>
    {
        Task<UserRolesDto> GetUserRoleByUser(Guid userid);
    }
}