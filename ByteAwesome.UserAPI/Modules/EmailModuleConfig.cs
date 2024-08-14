namespace ByteAwesome.UserAPI.Modules
{
    public class MailGunModuleConfig
    {
        public string URL { get; set; }
        public string apiKey { get; set; }
        public string domain { get; set; }
        public string ownerEmail { get; set; }
    }
    public class GMailModuleConfig
    {
        public string smtphost { get; set; }
        public string smtpport { get; set; }
        public string fromId { get; set; }
        public string fromPwd { get; set; }
    }
    public class BrevoMailModuleConfig
    {
        public string URL { get; set; }
        public string apiKey { get; set; }
        public string ownerEmail { get; set; }
    }
}
