using System.Text;
using Amazon;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Amazon.Runtime;
using ByteAwesome.SecretAPI.Modules;
using Microsoft.Extensions.Options;

namespace ByteAwesome.WalletAPI.ExternalApis.AwsKmsServices
{
    public interface IAwsKmsServices
    {
        Task<string> DecryptAsync(string encryptedText);
        Task<string> EncryptAsync(string textToEncrypt);
    }

    public class AwsKmsServices : IAwsKmsServices
    {
        private readonly Lazy<AmazonKeyManagementServiceClient> kmsClient;
        private readonly IOptions<AppModuleConfig> appConfig;
        private readonly IOptions<AwsModuleConfig> awsConfig;

        public AwsKmsServices(IOptions<AppModuleConfig> appConfig, IOptions<AwsModuleConfig> awsConfig)
        {
            this.appConfig = appConfig;
            this.awsConfig = awsConfig;
            kmsClient = new Lazy<AmazonKeyManagementServiceClient>(InitializeKmsClient, true);
        }

        public async Task<string> EncryptAsync(string textToEncrypt)
        {
            var encryptRequest = new EncryptRequest
            {
                KeyId = appConfig.Value.KmsPrivateKeyId,
                Plaintext = new MemoryStream(Encoding.UTF8.GetBytes(textToEncrypt))
            };
            var encryptResponse = await kmsClient.Value.EncryptAsync(encryptRequest);
            return Convert.ToBase64String(encryptResponse.CiphertextBlob.ToArray());
        }

        public async Task<string> DecryptAsync(string encryptedText)
        {
            var decryptRequest = new DecryptRequest
            {
                CiphertextBlob = new MemoryStream(Convert.FromBase64String(encryptedText))
            };
            var decryptResponse = await kmsClient.Value.DecryptAsync(decryptRequest);
            return Encoding.UTF8.GetString(decryptResponse.Plaintext.ToArray());
        }

        private AmazonKeyManagementServiceClient InitializeKmsClient()
        {
            if (GeneralHelper.IsDevelopmentEnvironment())
            {
                var credentials = new BasicAWSCredentials(awsConfig.Value.AccessKey, awsConfig.Value.SecretKey);
                var config = new AmazonKeyManagementServiceConfig
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(awsConfig.Value.Region)
                };
                return new AmazonKeyManagementServiceClient(credentials, config);
            }
            return new AmazonKeyManagementServiceClient();
        }
    }
}
