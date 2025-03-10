using System.ComponentModel.DataAnnotations;using AwesomeProject;

namespace UserAPI.Models.Dtos
{
    public abstract class UserIdentityProfileDto
    {
        [Required]
        [RegularExpression(@"^(?=[a-zA-Z0-9._]{1,20}$)(?!.*[_.]{2})[^_.].*[^_.]$", ErrorMessage = "must be 1-20 characters long, contain only letters, numbers, dot, or underscores, not start or end with a dot or underscore, and not have consecutive dots or underscores.")]
        public string UserName { get; set; }
        [RegularExpression(@"^\+(?:[0-9] ?){6,14}[0-9]$")]
        public string PhoneNumber { get; set; }
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
    }

    public class MaskedUserIdentityProfileDto(UserDto source)
    {
        public Guid? UserId { get; set; } = source.Id;
        public string UserName { get; set; } = source.UserData.UserName;
        public string EmailAddress { get; set; } = GeneralHelper.MaskEmail(source.UserData.EmailAddress);
        public string PhoneNumber { get; set; } = GeneralHelper.MaskPhoneNumber(source.UserData.PhoneNumber);
    }
}