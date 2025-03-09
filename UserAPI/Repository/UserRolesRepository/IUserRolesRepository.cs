using AwesomeProject;
using UserAPI.Models.Dtos;

namespace UserAPI.Repository
{
    public interface IUserRolesRepository : IBaseRepository<UserRolesDto, CreateUserRolesDto, Guid>
    {
        Task<UserRolesDto> GetUserRoleByUser(Guid userid);
    }
}