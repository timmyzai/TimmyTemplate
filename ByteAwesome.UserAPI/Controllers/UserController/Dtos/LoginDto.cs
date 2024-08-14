using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ByteAwesome.UserAPI.Models.Dtos
{
    public class LoginIdentityDto
    {
        // Email, Phone Number, Username
        [Required]
        public string UserLoginIdentityAddress { get; set; }
    }
    public class LoginDto : LoginIdentityDto
    {
        [Required]
        public string Password { get; set; }
        [StringLength(6, ErrorMessage = "The {0} must be {1} characters long.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The field {0} must consist of exactly 6 digits")]
        public string TwoFactorPin { get; set; }

        [StringLength(6, ErrorMessage = "The {0} must be {1} characters long.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The field {0} must consist of exactly 6 digits")]
        public string EmailTacCode { get; set; }
    }
    public class LoginResultDto
    {
        public MaskedUserIdentityProfileDto User { get; set; }
        public string EncryptedAccessToken { get; set; }
        public int ExpireInSeconds { get; set; }
        public string PublicKey { get; set; }
    }
    public class RefreshLoginTokenResultDto
    {
        public string EncryptedAccessToken { get; set; }
        public int ExpireInSeconds { get; set; }
    }
    public enum LoginResultType
    {
        [Description("Login Successfully")]
        Success = 1,
        [Description("Invalid user identity value during login")]
        InvalidUserLoginIdentity = 2,
        [Description("Invalid password during login")]
        InvalidPassword = 3,
        [Description("User is inactive")]
        UserIsNotActive = 4,
        [Description("User is locked out")]
        LockedOut = 5,
        [Description("Require Otp 6 digit Pin to verify email")]
        RequireOtpToVerifyEmail = 6,
        [Description("Require TFA Pin")]
        RequireTwoFactorPin = 7,
        [Description("Require Login Otp Pin")]
        RequireLoginOtp = 8,
        [Description("This action requires to enable PassKey")]
        PassKeyNotEnabled = 9,
        [Description("Require Login Passkey")]
        RequiredPasskey = 10,
    }
    public class LogOutDto
    {
        public List<UserLoginSessionInfoDto> UserLoginSessionList { get; set; }
    }
}