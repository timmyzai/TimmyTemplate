using ByteAwesome.SecretAPI.Grpc;

namespace ByteAwesome.UserAPI.GrpcClient
{
    public interface ITfaGrpcClient
    {
        Task<TwoFactorAuthProtoDto> CreateOrUpdateTwoFactorAuth(Guid userId, string userName, string languageCode = "en");
        Task<bool> VerifyTwoFactorPin(Guid userId, string twoFactorPin);
    }

    public class TfaGrpcClient : ITfaGrpcClient
    {
        private readonly TfaGrpcService.TfaGrpcServiceClient client;

        public TfaGrpcClient(TfaGrpcService.TfaGrpcServiceClient client)
        {
            this.client = client;
        }
        public async Task<TwoFactorAuthProtoDto> CreateOrUpdateTwoFactorAuth(Guid userId, string userName, string languageCode = "en")
        {
            try
            {
                var param = new CreateOrUpdateTwoFactorAuthProtoDto()
                {
                    UserId = userId.ToString(),
                    UserName = userName,
                    LanguageCode = languageCode
                };
                var response = await client.CreateOrUpdateTwoFactorAuthAsync(param);
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
                return response;
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                throw new Exception("An error occurred while processing your request. Please try again later.");
            }
        }
        public async Task<bool> VerifyTwoFactorPin(Guid userId, string twoFactorPin)
        {
            try
            {
                var param = new VerifyTwoFactorPinProtoDto()
                {
                    UserId = userId.ToString(),
                    TwoFactorPin = twoFactorPin
                };
                var response = await client.VerifyTwoFactorPinAsync(param);
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
                return response.Success;
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                throw new Exception("An error occurred while processing your request. Please try again later.");
            }
        }
    }
}
