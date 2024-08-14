using AutoMapper;
using ByteAwesome.UserAPI.DbContexts;
using ByteAwesome.UserAPI.Models;
using ByteAwesome.UserAPI.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using ByteAwesome.UserAPI.Helper;

namespace ByteAwesome.UserAPI.Repository
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
            if (input.KycData is not null)
            {
                item.KycData = mapper.Map<Kyc>(input.KycData);
                context.Set<Kyc>().Add(item.KycData);
                await context.SaveChangesAsync();
            }
            return mapper.Map<UserDto>(item); //make sure it is not overriden
        }
        public async Task CheckIfUserExists(string UserName, string PhoneNumber, string EmailAddress)
        {
            var item = await Query().FirstOrDefaultAsync(u => u.UserName == UserName || u.PhoneNumber == PhoneNumber || u.EmailAddress == EmailAddress);
            if (item is not null)
            {
                throw new AppException(ErrorCodes.User.IdentityTypeAlreadyExists);
            }
        }
        public async Task<UserDto> GetUserByUserLoginIdentity(string userLoginIdentity)
        {
            var item = await Query().FirstOrDefaultAsync(u => !u.IsDeleted && (u.UserName == userLoginIdentity || u.EmailAddress == userLoginIdentity || u.PhoneNumber == userLoginIdentity));
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
            return base.Query( isBoRequest)
                            .Include(x => x.Roles)
                            .Include(x => x.KycData)
                            .AsQueryable();
        }
    }
}