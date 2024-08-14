using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ByteAwesome.UserAPI.Models.Dtos
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

    public class MaskedUserIdentityProfileDto
    {
        public Guid? UserId { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }

        public MaskedUserIdentityProfileDto(UserDto source)
        {
            UserName = source.UserData.UserName;
            EmailAddress = source.UserData.EmailAddress;
            PhoneNumber = MaskString(source.UserData.PhoneNumber);
        }

        private string MaskString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            if (input.Length <= 6)
            {
                return new string('*', input.Length);
            }

            string firstThree = input.Substring(0, 3);
            string lastThree = input.Substring(input.Length - 3, 3);
            string maskedMiddle = new string('*', input.Length - 6);

            return firstThree + maskedMiddle + lastThree;
        }
    }


    public class UserBasicInfoDto : UserIdentityProfileDto
    {
        [JsonIgnore]
        public new string PhoneNumber { get; set; }
        public Guid UserId { get; set; }
    }
}