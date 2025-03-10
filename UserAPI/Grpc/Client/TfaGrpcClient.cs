using SecretAPI.Grpc;
using AwesomeProject;

namespace UserAPI.GrpcClient
{
    public interface ITfaGrpcClient
    {
        Task<TwoFactorAuthProtoDto> CreateOrUpdateTwoFactorAuth(Guid userId, string userName);
        Task<VerifyTwoFactorPinResponse> VerifyTwoFactorPin(Guid userId, string twoFactorPin, bool isProxy = false);
    }

    public class TfaGrpcClient : ITfaGrpcClient
    {
        private readonly TfaGrpcService.TfaGrpcServiceClient client;

        public TfaGrpcClient(TfaGrpcService.TfaGrpcServiceClient client)
        {
            this.client = client;
        }
        public async Task<TwoFactorAuthProtoDto> CreateOrUpdateTwoFactorAuth(Guid userId, string userName)
        {
            var param = new CreateOrUpdateTwoFactorAuthProtoDto()
            {
                UserId = userId.ToString(),
                UserName = userName,
            };
            var response = await client.CreateOrUpdateTwoFactorAuthAsync(param);
            GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
            return response;
        }
        public async Task<VerifyTwoFactorPinResponse> VerifyTwoFactorPin(Guid userId, string twoFactorPin, bool isProxy = false)
        {
            var param = new VerifyTwoFactorPinProtoDto()
            {
                UserId = userId.ToString(),
                TwoFactorPin = twoFactorPin
            };
            var response = await client.VerifyTwoFactorPinAsync(param);
            GrpcResponseValidator.ValidateResponse(response.ErrorMessage, fireAndForget: isProxy);
            return response;
        }
    }
}
