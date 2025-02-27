using AutoMapper;
using ByteAwesome.TestAPI.DbContexts;
using ByteAwesome.TestAPI.Dtos.User;
using ByteAwesome.TestAPI.Models;

namespace ByteAwesome.TestAPI.Repositories;

public interface IUserRepository : IBaseRepository<UserDto,CreateUserDto,Guid>
{
};

public class UserRepository : BaseRepository<User,UserDto,CreateUserDto,Guid>, IUserRepository
{
    public UserRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}