using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ByteAwesome.UserAPI.Models.Dtos
{
    public class CreateUserDto : UserProfileDto
    {
        [Required]
        [StringLength(15, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^(?=(.*[a-z]){1,})(?=(.*[A-Z]){1,})(?=(.*\d){1,})(?=(.*[\W_]){1,})[a-zA-Z\d\W_]{6,}$", ErrorMessage = "Passwords must be at least 6 characters and contain the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
        public string PassWord { get; set; }
        public List<Guid>? RoleIdList { get; set; }
        [JsonIgnore]
        public KycDto KycData { get; set; } = new KycDto();
    }
    public class Bo_UserDataDto : UserDataDto
    {
        public string PassWord { get; set; }
        public string PasswordSalt { get; set; }
        public bool IsActive { get; set; }
        public bool IsLockedOut { get; set; }
        public int PasswordTryCount { get; set; }
        public IList<string> RoleNames { get; set; } = new List<string>();
    }

    public class UserDto : FullyAuditedEntityDto<Guid>
    {
        public Bo_UserDataDto UserData { get; set; }
    }
}