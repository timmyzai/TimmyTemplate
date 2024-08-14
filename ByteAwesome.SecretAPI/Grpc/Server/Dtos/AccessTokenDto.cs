namespace ByteAwesome.SecretAPI.Models.Dtos
{
    public class GenerateAccessTokenResult
    {
        public string EncryptedAccessToken { get; set; }
        public int ExpireInSeconds { get; set; }
    }
}