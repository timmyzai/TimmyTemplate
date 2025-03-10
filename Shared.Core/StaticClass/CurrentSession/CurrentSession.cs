using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace AwesomeProject
{
    public static class CurrentSession
    {
        private static IHttpContextAccessor _httpContextAccessor;
        private const string CurrentUserKey = "CurrentUser";

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static void SetUser(HttpContext context, Guid userId, ClaimsPrincipal user)
        {
            context.Items[CurrentUserKey] = new CurrentUser
            {
                Id = userId,
                UserName = user.FindFirstValue(ClaimTypes.Name),
                EmailAddress = user.FindFirstValue(ClaimTypes.Email),
                PhoneNumber = user.FindFirstValue(ClaimTypes.MobilePhone),
                RoleNames = user.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList()
            };
        }

        public static CurrentUser GetUser()
        {
            var currentUser = _httpContextAccessor?.HttpContext?.Items[CurrentUserKey] as CurrentUser;
            if (currentUser == null)
                throw new AppException(ErrorCodes.General.PleaseLogin);

            return currentUser;
        }
        public static Guid? GetUserId()
            => (_httpContextAccessor?.HttpContext?.Items[CurrentUserKey] as CurrentUser)?.Id;

        public static string GetUserName()
            => (_httpContextAccessor?.HttpContext?.Items[CurrentUserKey] as CurrentUser)?.UserName;

        public static string GetEmailAddress()
            => (_httpContextAccessor?.HttpContext?.Items[CurrentUserKey] as CurrentUser)?.EmailAddress;

        public static string GetPhoneNumber()
            => (_httpContextAccessor?.HttpContext?.Items[CurrentUserKey] as CurrentUser)?.PhoneNumber;

        public static IList<string> GetUserRoles()
            => (_httpContextAccessor?.HttpContext?.Items[CurrentUserKey] as CurrentUser)?.RoleNames;

        public static string GetUserLoginSessionId()
            => _httpContextAccessor?.HttpContext?.User.FindFirst("UserLoginSessionId")?.Value;

        public static string GetUserLanguage()
            => _httpContextAccessor?.HttpContext?.Items["UserLanguage"] as string ?? "en";

        public class CurrentUser : UserIdentityProfileDto
        {
            public Guid Id { get; set; }
        }
    }
}
