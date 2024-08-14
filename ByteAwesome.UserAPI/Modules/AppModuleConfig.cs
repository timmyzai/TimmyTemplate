namespace ByteAwesome.UserAPI.Modules
{
    public class AppModuleConfig
    {
        public string ServerRootAddress { get; set; }
        public string ClientRootAddress { get; set; }
        public string EncryptSecretKey { get; set; }
        public string UploadRootFolder { get; set; }
        public string UploadDB { get; set; }
        public bool IsComingSoon { get; set; }
    }
}
