namespace ByteAwesome.SecretAPI.Modules
{
    public class AppModuleConfig
    {
        public string ServerRootAddress { get; set; }
        public string ClientRootAddress { get; set; }
        public string EncryptSecretKey { get; set; }
        public string KmsPrivateKeyId { get; set; }
        public string TacCodeExpiryTime { get; set; }
    }
}
