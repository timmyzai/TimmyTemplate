

namespace ByteAwesome.SecretAPI.Models.Dtos
{
    public class KmsKeysDto : AuditedEntityDto<Guid>
    {
        public Guid UserId { get; set; }
        public string EncryptedPrivateKey { get; set; }
        public string PublicKey { get; set; }
    }
}
