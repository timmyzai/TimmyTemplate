using System.ComponentModel.DataAnnotations;

namespace ByteAwesome.UserAPI.Models.Dtos
{
    public class VerifyEmailDto
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
