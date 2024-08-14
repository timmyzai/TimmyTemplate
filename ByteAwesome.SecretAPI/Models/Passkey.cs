namespace ByteAwesome.SecretAPI.Models
{
    public class Passkey : Entity<Guid>
    {        
        public byte[] CredentialId { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] UserHandle { get; set; }
        public uint SignatureCounter { get; set; }
        public string UserName { get; set; }
    }
}