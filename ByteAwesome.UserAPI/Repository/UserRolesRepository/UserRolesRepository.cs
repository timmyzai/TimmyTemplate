using AutoMapper;
using ByteAwesome.UserAPI.DbContexts;
using ByteAwesome.UserAPI.Models;
using ByteAwesome.UserAPI.Models.Dtos;
using Microsoft.EntityFrameworkCore;
namespace ByteAwesome.UserAPI.Repository
{
    public class UserRolesRepository : BaseRepository<UserRoles, UserRolesDto, CreateUserRolesDto, Guid>, IUserRolesRepository
    {
        public UserRolesRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper) { }
        public async Task<UserRolesDto> GetUserRoleByUser(Guid userid)
        {
            var result = await context.Set<UserRoles>().FirstOrDefaultAsync(x => x.UsersId == userid);
            return mapper.Map<UserRolesDto>(result);
        }
    }
}