using System.Security.Claims;
using ByteAwesome.Services;
using Microsoft.AspNetCore.Http;

namespace ByteAwesome
{
    public static class CurrentSession
    {
        private static IHttpContextAccessor _httpContextAccessor;
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public static CurrentUser GetUser()
        {
            return new CurrentUser()
            {
                Id = GetUserId(),
                UserName = GetUserName(),
                EmailAddress = GetEmailAddress(),
                PhoneNumber = GetPhoneNumber()
            };
        }
        public static string GetCurrentToken()
        {
            var authorizationHeader = _httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
            {
                return authorizationHeader.Substring(7);
            }
            return null;
        }
        public static DeviceInfo GetUserDeviceInfo()
        {
            var deviceInfo = _httpContextAccessor?.HttpContext?.Items["DeviceInfo"] as DeviceInfo;
            if (deviceInfo is null)
            {
                return new DeviceInfo();
            }
            return deviceInfo;
        }
        public static LocationInfo GetUserLocationInfo()
        {
            var locationInfo = _httpContextAccessor?.HttpContext?.Items["GeoLocation"] as LocationInfo;
            if (locationInfo is null)
            {
                return new LocationInfo();
            }
            return locationInfo;
        }
        public static Guid GetUserId()
        {
            var _claim = _httpContextAccessor?.HttpContext?.User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier);
            if (_claim is not null && Guid.TryParse(_claim.Value, out Guid userId) && userId != Guid.Empty)
            {
                return userId;
            }
            throw new AppException(ErrorCodes.General.PleaseLogin);
        }
        public static string GetUserName()
        {
            var _claim = _httpContextAccessor?.HttpContext?.User.FindFirst(x => x.Type == ClaimTypes.Name);
            if (_claim is not null && !String.IsNullOrEmpty(_claim.Value))
            {
                return _claim.Value;
            };
            return null;
        }
        public static string GetEmailAddress()
        {
            var _claim = _httpContextAccessor?.HttpContext?.User.FindFirst(x => x.Type == ClaimTypes.Email);
            if (_claim is not null && !String.IsNullOrEmpty(_claim.Value))
            {
                return _claim.Value;
            };
            return null;
        }
        public static string GetPhoneNumber()
        {
            var _claim = _httpContextAccessor?.HttpContext?.User.FindFirst(x => x.Type == ClaimTypes.MobilePhone);
            if (_claim is not null && !String.IsNullOrEmpty(_claim.Value))
            {
                return _claim.Value;
            };
            return null;
        }
        public static string GetUserRoleName()
        {
            var _claim = _httpContextAccessor?.HttpContext?.User.FindFirst(x => x.Type == ClaimTypes.Role);
            if (_claim is not null && !String.IsNullOrEmpty(_claim.Value))
            {
                return _claim.Value;
            };
            return null;
        }
        public static string GetUserLoginSessionId()
        {
            var _claim = _httpContextAccessor?.HttpContext?.User.FindFirst(x => x.Type == "UserLoginSessionId");
            if (_claim is not null && !String.IsNullOrEmpty(_claim.Value))
            {
                return _claim.Value;
            };
            return null;
        }
        public static string GetUserLanguage()
        {
            var _languageCode = _httpContextAccessor?.HttpContext?.Items?["UserLanguage"] as string;
            if (_languageCode is not null)
            {
                return _languageCode;
            }
            return "en";
        }
        public class CurrentUser : UserIdentityProfileDto
        {
            public Guid Id { get; set; }
        }
    }
}
