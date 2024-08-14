

namespace ByteAwesome.SecretAPI.Models
{
    public class KmsKeys : AuditedEntity<Guid>
    {
        public Guid UserId { get; set; }
        public string EncryptedPrivateKey { get; set; }
        public string PublicKey { get; set; }
    }
}