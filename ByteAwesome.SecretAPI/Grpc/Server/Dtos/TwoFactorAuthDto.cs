using System.ComponentModel.DataAnnotations;

namespace ByteAwesome.SecretAPI.Models.Dtos
{
    public class CreateTwoFactorAuthDto
    {
        [Required]
        public Guid? UserId { get; set; }
        public string TwoFactorSecretKey { get; set; }
        public string TwoFactorQrImgUrl { get; set; }
        public string TwoFactorManualEntryKey { get; set; }
    }
    public class TwoFactorAuthDto : AuditedEntityDto<Guid>
    {
        [Required]
        public Guid? UserId { get; set; }
        public string TwoFactorSecretKey { get; set; }
        public string TwoFactorQrImgUrl { get; set; }
        public string TwoFactorManualEntryKey { get; set; }
    }
}