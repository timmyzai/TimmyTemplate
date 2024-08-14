using System.ComponentModel.DataAnnotations;

namespace ByteAwesome.UserAPI.Models.Dtos
{
    public abstract class ChangePasswordDto
    {
        [StringLength(15, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^(?=(.*[a-z]){1,})(?=(.*[A-Z]){1,})(?=(.*\d){1,})(?=(.*[\W_]){1,})[a-zA-Z\d\W_]{6,}$", ErrorMessage = "Passwords must be at least 6 characters and contain the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
        [Required]
        public string NewPassword { get; set; }
        [StringLength(15, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^(?=(.*[a-z]){1,})(?=(.*[A-Z]){1,})(?=(.*\d){1,})(?=(.*[\W_]){1,})[a-zA-Z\d\W_]{6,}$", ErrorMessage = "Passwords must be at least 6 characters and contain the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        [Required]
        public string ConfirmPassword { get; set; }
    }
    public class ChangeUserPasswordDto : ChangePasswordDto
    {
        [Required]
        public string TwoFactorPin { get; set; }
    }
    public class ChangeUserPasswordByEmailDto : ChangePasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [StringLength(6, ErrorMessage = "The {0} must be {1} characters long.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The field {0} must consist of exactly 6 digits")]
        public string TacCode { get; set; }
    }
}
