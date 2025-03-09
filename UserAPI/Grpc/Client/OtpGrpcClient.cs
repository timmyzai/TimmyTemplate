using SecretAPI.Grpc;
using AwesomeProject;

namespace UserAPI.GrpcClient
{
    public interface IOtpGrpcClient
    {
        Task<OtpProtoDto> Add(string emailOrPhoneNumber, OtpProtoType otpType, OtpActionProtoType actionType, bool isLogin = false);
        Task Verify(Guid userId, string emailOrPhoneNumber, string tacCode, OtpActionProtoType actionType);
    }

    public class OtpGrpcClient : IOtpGrpcClient
    {
        private readonly OtpGrpcService.OtpGrpcServiceClient client;

        public OtpGrpcClient(
            OtpGrpcService.OtpGrpcServiceClient client
        )
        {
            this.client = client;
        }
        public async Task<OtpProtoDto> Add(string emailOrPhoneNumber, OtpProtoType otpType, OtpActionProtoType actionType, bool isLogin = false)
        {
            var request = new CreateOtpProtoDto
            {
                Value = emailOrPhoneNumber,
                Type = otpType,
                ActionType = actionType
            };
            var response = await client.AddAsync(request);
            GrpcResponseValidator.ValidateResponse(response.ErrorMessage, fireAndForget: isLogin); //is login is true, then no need to throw exception
            return response;
        }
        public async Task Verify(Guid userId, string emailOrPhoneNumber, string tacCode, OtpActionProtoType actionType)
        {
            var request = new VerifyOTPProtoDto
            {
                UserId = userId.ToString(),
                Value = emailOrPhoneNumber,
                TacCode = tacCode,
                ActionType = actionType
            };
            var response = await client.VerifyAsync(request);
            GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
        }
    }
}
