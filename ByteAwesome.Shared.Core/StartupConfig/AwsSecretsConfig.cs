using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Newtonsoft.Json;

namespace ByteAwesome.StartupConfig
{
    public partial class StartUpHelper
    {
        public class AwsSecretsConfig : IConfigurationSource
        {
            private readonly string _secretId;
            private readonly string _region;

            public AwsSecretsConfig(IConfiguration configuration)
            {
                _secretId = configuration["AWS:SecretManager:SecretId"];
                _region = configuration["AWS:SecretManager:Region"];
                ValidateConfig();
            }

            private void ValidateConfig()
            {
                if (string.IsNullOrEmpty(_secretId) || string.IsNullOrEmpty(_region))
                    throw new ArgumentException("AWS configuration for 'SecretId' or 'Region' is missing in the appsettings.json.");
            }

            public IConfigurationProvider Build(IConfigurationBuilder builder)
            {
                try
                {
                    var awsClient = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(_region));
                    var secretValueTask = awsClient.GetSecretValueAsync(new GetSecretValueRequest { SecretId = _secretId });
                    GetSecretValueResponse response;


                    response = secretValueTask.GetAwaiter().GetResult();

                    if (string.IsNullOrEmpty(response.SecretString))
                        throw new Exception("No secrets returned from AWS Secrets Manager.");

                    var secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.SecretString);
                    var memoryConfigSource = new MemoryConfigurationSource { InitialData = secrets };
                    return new MemoryConfigurationProvider(memoryConfigSource);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to retrieve AWS secrets due to: {ex.Message}", ex);
                }
            }
        }
    }
}

