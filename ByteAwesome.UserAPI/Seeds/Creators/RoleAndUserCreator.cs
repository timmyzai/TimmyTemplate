using Microsoft.EntityFrameworkCore;
using ByteAwesome.UserAPI.Models;
using ByteAwesome.UserAPI.DbContexts;
using ByteAwesome.UserAPI.Helper;

namespace ByteAwesome.UserAPI.Seed.RoleAndUser
{
    public class RoleAndUserCreator
    {
        private readonly ApplicationDbContext context;

        public RoleAndUserCreator(ApplicationDbContext context)
        {
            this.context = context;
        }

        public void Create()
        {
            CreateRoleAndUserCreator(UserNames.AdminUserName, RoleNames.Admin, "123qwe");
            CreateRoleAndUserCreator(UserNames.ClientsUserName, RoleNames.Clients, Password: "123qwe");
        }

        private UserRoles CreateRoleAndUserCreator(string UserName, string UserRole, string Password)
        {
            var adminRole = context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.Name == UserRole);
            if (adminRole is null)
            {
                adminRole = context.Roles.Add(new Roles() { Name = UserRole }).Entity;
                context.SaveChanges();
            }
            var adminUser = context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.UserName == UserName);
            if (adminUser is null)
            {
                var item = new Users
                {
                    UserName = UserName,
                    LastName = UserName,
                    FirstName = "byteawesome",
                    PhoneNumber = $"+60123456789",
                    EmailAddress = $"{UserName}@byteawesome.com",
                };
                var (hash, salt) = PwdHashTokenHelper.CreateHash(Password);
                item.PassWord = hash;
                item.PasswordSalt = salt;
                adminUser = context.Users.Add(item).Entity;
                context.SaveChanges();
                var itemUser = context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.UserName == UserName);
                if (itemUser?.Id is not null)
                {
                    var userRoles = new UserRoles
                    {
                        UsersId = itemUser.Id,
                        RolesId = adminRole.Id,
                    };
                    var adminUserRoles = context.UserRoles.Add(userRoles).Entity;
                    context.SaveChanges();
                }
            }
            return default;
        }
    }
}
