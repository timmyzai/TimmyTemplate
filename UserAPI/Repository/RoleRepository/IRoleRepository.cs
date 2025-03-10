using UserAPI.Models.Dtos;
using AwesomeProject;

namespace UserAPI.Repository
{
    public interface IRoleRepository : IBaseRepository<RolesDto, CreateRolesDto, Guid>
    {
        Task<RolesDto> GetRoleByName(string roleName);
    }
}
