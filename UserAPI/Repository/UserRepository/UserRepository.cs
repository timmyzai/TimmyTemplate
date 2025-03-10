using AutoMapper;
using UserAPI.DbContexts;
using UserAPI.Models;
using UserAPI.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using UserAPI.Helper;
using AwesomeProject;

namespace UserAPI.Repository
{
    public class UserRepository : BaseRepository<Users, UserDto, CreateUserDto, Guid>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper) { }
        public async override Task<UserDto> Add(CreateUserDto input)
        {
            var item = mapper.Map<Users>(input);
            var (hash, salt) = PwdHashTokenHelper.CreateHash(item.PassWord);
            item.PassWord = hash;
            item.PasswordSalt = salt;
            context.Add(item);
            await context.SaveChangesAsync();
            return mapper.Map<UserDto>(item); //make sure it is not overriden
        }
        public async Task CheckIfUserExists(string UserName, string PhoneNumber, string EmailAddress)
        {
            if (string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(PhoneNumber) && string.IsNullOrEmpty(EmailAddress)) throw new AppException(ErrorCodes.User.InvalidUserLoginIdentity);
            var item = await Query(isBoRequest: true).FirstOrDefaultAsync(u => u.UserName == UserName || u.EmailAddress == EmailAddress || (u.IsPhoneVerified && u.PhoneNumber == PhoneNumber));
            if (item is not null)
            {
                if (string.Equals(item.UserName, UserName, StringComparison.OrdinalIgnoreCase))
                    throw new AppException(ErrorCodes.User.UserNameAlreadyExists);
                if (string.Equals(item.EmailAddress, EmailAddress, StringComparison.OrdinalIgnoreCase))
                    throw new AppException(ErrorCodes.User.EmailAddressAlreadyExists);
                if (item.IsPhoneVerified && string.Equals(item.PhoneNumber, PhoneNumber, StringComparison.OrdinalIgnoreCase))
                    throw new AppException(ErrorCodes.User.PhoneNumberAlreadyExists);
            }
        }
        public async Task<UserDto> GetUserByUserLoginIdentity(string userLoginIdentity)
        {
            if (string.IsNullOrEmpty(userLoginIdentity)) throw new AppException(ErrorCodes.User.InvalidUserLoginIdentity);
            var item = await Query().FirstOrDefaultAsync(u => u.UserName == userLoginIdentity || u.EmailAddress == userLoginIdentity || (u.IsPhoneVerified && u.PhoneNumber == userLoginIdentity));
            if (item is null)
            {
                return default;
            }
            return MapEntityToDto(item);
        }
        protected override UserDto MapEntityToDto(Users user)
        {
            var roleIds = user.Roles.Select(x => x.RolesId).ToList();
            var roles = context.Set<Roles>().Where(r => roleIds.Contains(r.Id)).Select(r => r.Name).ToList();
            var result = base.MapEntityToDto(user);
            result.UserData.RoleNames = roles;
            return result;
        }
        protected override IEnumerable<UserDto> MapEntitiesToDtos(IEnumerable<Users> userList)
        {
            var roleIds = userList.SelectMany(u => u.Roles.Select(r => r.RolesId)).Distinct().ToList();
            var roles = context.Set<Roles>().Where(r => roleIds.Contains(r.Id)).ToList();
            var roleNamesDict = roles.ToDictionary(r => r.Id, r => r.Name);
            var userDtos = base.MapEntitiesToDtos(userList);

            foreach (var userDto in userDtos)
            {
                var user = userList.First(u => u.Id == userDto.Id);
                var userRoleNames = user.Roles
                    .Select(r => roleNamesDict[r.RolesId])
                    .ToList();

                userDto.UserData.RoleNames = userRoleNames;
            }
            return userDtos;
        }
        protected override IQueryable<Users> Query(bool isBoRequest = false)
        {
            return base.Query(isBoRequest)
                            .Include(x => x.Roles)
                            .AsQueryable();
        }
    }
}