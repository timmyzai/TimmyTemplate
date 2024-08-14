namespace ByteAwesome.SecretAPI.Modules
{
    public class Fido2ModuleConfig
    {
        public string ServerDomain { get; set; }
        public string ServerName { get; set; }
        public List<string> Origins { get; } = new List<string>();
        public int TimestampDriftTolerance { get; set; }
    }
}
