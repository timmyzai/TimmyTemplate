using AutoMapper;
using TestAPI.DbContexts;
using TestAPI.Dtos.User;
using TestAPI.Models;

using AwesomeProject;

namespace TestAPI.Repositories;

public interface IUserRepository : IBaseRepository<UserDto,CreateUserDto,Guid>
{
};

public class UserRepository : BaseRepository<User,UserDto,CreateUserDto,Guid>, IUserRepository
{
    public UserRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}