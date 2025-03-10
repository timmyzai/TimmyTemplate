using Microsoft.EntityFrameworkCore;
using UserAPI.Models;
using UserAPI.DbContexts;
using UserAPI.Helper;
using AwesomeProject;

namespace UserAPI.Seed.RoleAndUser
{
    public partial class RoleAndUserSeedCreator
    {
        private readonly ApplicationDbContext context;

        public RoleAndUserSeedCreator(ApplicationDbContext context)
        {
            this.context = context;
        }

        public void Create()
        {
            CreateUser(UserNames.AdminUserName, RoleNames.Admin, "123qwe");
            CreateUser(UserNames.ClientsUserName, RoleNames.Clients, "123qwe");
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (env != "Production")
            {
                CreateUsersWithPrefix("qa", "test1234@", 3, 3);
            }
        }
        public void CreateUsersWithPrefix(string prefix, string password, int mainCount, int sobCount = 3)
        {
            for (int mainIndex = 1; mainIndex <= mainCount; mainIndex++)
            {
                string mainUserNumber = mainIndex.ToString("D2");
                string mainUserName = $"{prefix}_main{mainUserNumber}";
                string firstName = $"First{mainUserNumber}";
                string lastName = $"Last{mainUserNumber}";
                string email = $"{mainUserName}@testmail.com";
                string phoneNumber = GenerateRandomPhoneNumber();

                CreateUserWithRole(mainUserName, firstName, lastName, email, phoneNumber, password, RoleNames.Clients, isTwoFactorEnabled: true);

                for (int sobIndex = 1; sobIndex <= sobCount; sobIndex++)
                {
                    string sobUserName = $"{prefix}_sob{mainIndex}{sobIndex}";
                    string sobFirstName = $"First{mainIndex}{sobIndex}";
                    string sobLastName = $"Last{mainIndex}{sobIndex}";
                    string sobEmail = $"{sobUserName}@testmail.com";
                    string sobPhoneNumber = GenerateRandomPhoneNumber();

                    CreateUserWithRole(sobUserName, sobFirstName, sobLastName, sobEmail, sobPhoneNumber, password, RoleNames.Clients, isTwoFactorEnabled: true);
                }
            }
        }

        private void CreateUserWithRole(string userName, string firstName, string lastName, string email, string phoneNumber, string password, string roleName, bool isTwoFactorEnabled = false)
        {
            var role = context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.Name == roleName);
            if (role == null)
            {
                role = context.Roles.Add(new Roles { Name = roleName }).Entity;
                context.SaveChanges();
            }
            var user = context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.UserName == userName);
            if (user == null)
            {
                var newUser = new Users
                {
                    UserName = userName,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailAddress = email,
                    PhoneNumber = phoneNumber,
                    IsEmailVerified = true,
                    IsTwoFactorEnabled = isTwoFactorEnabled
                };
                var (hash, salt) = PwdHashTokenHelper.CreateHash(password);
                newUser.PassWord = hash;
                newUser.PasswordSalt = salt;

                user = context.Users.Add(newUser).Entity;
                context.SaveChanges();

                var userRole = new UserRoles
                {
                    UsersId = user.Id,
                    RolesId = role.Id,
                };
                context.UserRoles.Add(userRole);
                context.SaveChanges();
            }
        }

        private string GenerateRandomPhoneNumber()
        {
            var random = new Random();
            int numberLength = random.Next(6, 15);
            string number = "+";
            for (int i = 0; i < numberLength; i++)
            {
                number += random.Next(0, 10).ToString();
                if (random.NextDouble() > 0.8 && i != numberLength - 1)
                {
                    number += " ";
                }
            }
            return number;
        }

        private void CreateUser(string userName, string userRole, string password)
        {
            string firstName = "awesome";
            string lastName = userName;
            string email = $"{userName}@awesome.com";
            string phoneNumber = "+60123456789";
            CreateUserWithRole(userName, firstName, lastName, email, phoneNumber, password, userRole);
        }
    }
}
