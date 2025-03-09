using System.ComponentModel.DataAnnotations;

namespace UserAPI.Models.Dtos
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
    public class LoginResultDto : RefreshTokenLoginResultDto
    {
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpireDate { get; set; }
    }
    public class RefreshTokenLoginResultDto
    {
        public string EncryptedAccessToken { get; set; }
        public DateTime AccessTokenExpireDate { get; set; }
    }
    public class RefreshSessionDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}