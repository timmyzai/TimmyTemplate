using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ByteAwesome.UserAPI.Models.Dtos
{
    public class UserProfileDto : UserIdentityProfileDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
    }
    public class RegisterDto : CreateUserDto
    {
        [JsonIgnore]
        public List<Guid>? RoleIdList { get; set; }
    }
    public class UserDataDto : UserProfileDto
    {
        public bool IsTwoFactorEnabled { get; set; }
        public bool IsPassKeyEnabled { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public string DisplayName { get { return FirstName + " " + LastName; } }
        public KycDto KycData { get; set; }
    }
    public class EntityUserDto : EntityDto<Guid>
    {
        public UserDataDto UserData { get; set; }
    }
    public class UpdateUserProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class UpdateUserEmailAddressDto : RequiredTwoFactorPin
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
    }
    public class UpdateUserPhoneNumberDto : RequiredTwoFactorPin
    {
        [Required]
        [RegularExpression(@"^\+(\d{1,3})(?:[ -]?\d){6,14}\d")]
        public string PhoneNumber { get; set; }
    }
    public class GetTwoFactorAuthInfoResult
    {
        public string TwoFactorSecretKey { get; set; }
        public string TwoFactorQrImgUrl { get; set; }
        public string TwoFactorManualEntryKey { get; set; }
    }
    public class EnableOrDisableTwoFactorAuthDto : RequiredTwoFactorPin
    {
        [Required]
        public bool Enable { get; set; }
    }
    public class DeleteMyAccountDto : RequiredTwoFactorPin { }
}
