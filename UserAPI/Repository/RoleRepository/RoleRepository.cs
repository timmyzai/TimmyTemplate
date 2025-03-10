using AutoMapper;
using UserAPI.DbContexts;
using UserAPI.Models;
using UserAPI.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using AwesomeProject;

namespace UserAPI.Repository
{
    public class RoleRepository : BaseRepository<Roles, RolesDto, CreateRolesDto, Guid>, IRoleRepository
    {
        public RoleRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper) { }
        public async Task<RolesDto> GetRoleByName(string name)
        {
            var item = await Query().FirstOrDefaultAsync(r => r.Name == name);
            if (item is null)
            {
                return default;
            }
            return MapEntityToDto(item);
        }
    }
}
