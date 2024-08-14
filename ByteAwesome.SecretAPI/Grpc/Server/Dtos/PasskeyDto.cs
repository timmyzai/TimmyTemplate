namespace ByteAwesome.SecretAPI.Models.Dtos
{
    public class PasskeyDto : EntityDto<Guid>
    {
        public byte[] CredentialId { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] UserHandle { get; set; }
        public uint SignatureCounter { get; set; }
        public string UserName { get; set; }
    }

    public class CreatePasskeyDto
    {
        public byte[] CredentialId { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] UserHandle { get; set; }
        public uint SignatureCounter { get; set; }
        public string UserName { get; set; }
    }
}