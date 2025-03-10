using AutoMapper;
using TestAPI.DbContexts;
using TestAPI.Dtos.User;
using TestAPI.Models;
using AwesomeProject;
using Microsoft.EntityFrameworkCore;

namespace TestAPI.Repositories;

public interface IUserRepository : IBaseRepository<UserDto, CreateUserDto, Guid>
{
    Task<string> GetUserCountryByUserId(Guid UserId);
}

public class UserRepository : BaseRepository<User, UserDto, CreateUserDto, Guid>, IUserRepository
{
    public UserRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
    public async Task<string> GetUserCountryByUserId(Guid UserId)
    {
        var countryName =  await ContextEntity().Where(x => x.Id == UserId).Select(x => x.CountryName).FirstOrDefaultAsync();
        return countryName;
    }
}