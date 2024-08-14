using ByteAwesome.UserAPI.Models.Dtos;

namespace ByteAwesome.UserAPI.Repository
{
    public interface IRoleRepository : IBaseRepository<RolesDto, CreateRolesDto, Guid>
    {
        Task<RolesDto> GetRoleByName(string roleName);
    }
}
