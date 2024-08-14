using ByteAwesome.SecretAPI.Grpc;

namespace ByteAwesome.UserAPI.GrpcClient
{
    public interface IKmsKeysGrpcClient
    {
        Task<CreateKmsKeyResultDto> CreateOrUpdateKmsKeys(Guid userId);
        Task<GetPublicKeyResultDto> GetPublicKey(Guid userId);
        Task<VerifySignAndDecryptResultDto> VerifyDataAndDecrypt(Guid userId, string encryptedData);
    }
    public class KmsKeysGrpcClient : IKmsKeysGrpcClient
    {
        private readonly KmsKeysGrpcService.KmsKeysGrpcServiceClient client;

        public KmsKeysGrpcClient(KmsKeysGrpcService.KmsKeysGrpcServiceClient client)
        {
            this.client = client;
        }
        public async Task<CreateKmsKeyResultDto> CreateOrUpdateKmsKeys(Guid userId)
        {
            try
            {
                var param = new CreateKmsKeyProtoDto()
                {
                    UserId = userId.ToString()
                };
                var response = await client.CreateOrUpdateKmsKeysAsync(param);
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
                return response;
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                throw new Exception("An error occurred while processing your request. Please try again later.");
            }
        }
        public async Task<GetPublicKeyResultDto> GetPublicKey(Guid userId)
        {
            try
            {
                var param = new GetPublicKeyProtoDto()
                {
                    UserId = userId.ToString()
                };
                var response = await client.GetPublicKeyAsync(param);
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
                return response;
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                throw new Exception("An error occurred while processing your request. Please try again later.");
            }
        }
        public async Task<VerifySignAndDecryptResultDto> VerifyDataAndDecrypt(Guid userId, string encryptedData)
        {

            try
            {
                var param = new VerifySignAndDecryptProtoDto()
                {
                    UserId = userId.ToString(),
                    EncryptedData = encryptedData
                };
                var response = await client.VerifyDataAndDecryptAsync(param);
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
                return response;
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                throw new Exception("An error occurred while processing your request. Please try again later.");
            }

        }
    }
}
