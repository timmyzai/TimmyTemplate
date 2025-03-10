

using AwesomeProject;

namespace SecretAPI.Models
{
    public class TwoFactorAuth : AuditedEntity<Guid>
    {
        public Guid UserId { get; set; }
        public string TwoFactorSecretKey { get; set; }
        public string TwoFactorQrImgUrl { get; set; }
        public string TwoFactorManualEntryKey { get; set; }
    }
}