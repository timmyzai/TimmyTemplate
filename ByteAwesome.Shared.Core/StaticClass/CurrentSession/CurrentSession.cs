using System.Security.Claims;
using ByteAwesome.Services;
using Microsoft.AspNetCore.Http;

namespace ByteAwesome
{
    public class CurrentSession
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
            var deviceInfo = _httpContextAccessor.HttpContext.Items["DeviceInfo"] as DeviceInfo;
            if (deviceInfo == null)
            {
                return new DeviceInfo();
            }
            return deviceInfo;
        }
        public static LocationInfo GetUserLocationInfo()
        {
            var locationInfo = _httpContextAccessor.HttpContext.Items["GeoLocation"] as LocationInfo;
            if (locationInfo == null)
            {
                return new LocationInfo();
            }
            return locationInfo;
        }
        public static Guid GetUserId()
        {
            var _user = _httpContextAccessor?.HttpContext?.User;
            var _claim = _user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (_claim != null && Guid.TryParse(_claim.Value, out Guid userId))
            {
                return userId;
            };
            throw new Exception($"{ErrorCodes.General.PleaseLogin} - {LanguageService.Translate(ErrorCodes.General.PleaseLogin)}");
        }
        public static string GetUserName()
        {
            var _user = _httpContextAccessor?.HttpContext?.User;
            var _claim = _user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            if (_claim != null && !String.IsNullOrEmpty(_claim.Value))
            {
                return _claim.Value;
            };
            return null;
        }
        public static string GetEmailAddress()
        {
            var _user = _httpContextAccessor?.HttpContext?.User;
            var _claim = _user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
            if (_claim != null && !String.IsNullOrEmpty(_claim.Value))
            {
                return _claim.Value;
            };
            return null;
        }
        public static string GetPhoneNumber()
        {
            var _user = _httpContextAccessor?.HttpContext?.User;
            var _claim = _user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.MobilePhone);
            if (_claim != null && !String.IsNullOrEmpty(_claim.Value))
            {
                return _claim.Value;
            };
            return null;
        }
        public static string GetUserRoleName()
        {
            var _user = _httpContextAccessor?.HttpContext?.User;
            var _claim = _user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            if (_claim != null && !String.IsNullOrEmpty(_claim.Value))
            {
                return _claim.Value;
            };
            return null;
        }
        public static string GetUserLoginSessionId()
        {
            var _user = _httpContextAccessor?.HttpContext?.User;
            var _claim = _user?.Claims.FirstOrDefault(x => x.Type == "UserLoginSessionId");
            if (_claim != null && !String.IsNullOrEmpty(_claim.Value))
            {
                return _claim.Value;
            };
            return null;
        }
        public class CurrentUser : UserIdentityProfileDto
        {
            public Guid? Id { get; set; }
        }
    }
}
