using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ByteAwesome
{
    public abstract class UserIdentityProfileDto
    {
        [Required]
        public string UserName { get; set; }
        [RegularExpression(@"^\+(?:[0-9] ?){6,14}[0-9]$")]
        public string PhoneNumber { get; set; }
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
        [JsonIgnore]
        public IList<string> RoleNames { get; set; }
    }
    public class UserBasicInfoDataDto : UserIdentityProfileDto
    {
        public UserRanks Rank { get; set; }
    }
    public class UserBasicInfoDto : FullyAuditedEntityDto<Guid>
    {
        public UserBasicInfoDataDto UserBasicInfoData { get; set; }
    }
    public enum UserRanks
    {
        Free = 0,
        Team = 1,
        Enterprise = 2,
        SuperAdmin = 999,
    }
    public class UpgradeRankDto
    {
        public UserRanks Rank { get; set; }
        [JsonIgnore]
        public Guid? PaymentOrderId { get; set; }

    }
}