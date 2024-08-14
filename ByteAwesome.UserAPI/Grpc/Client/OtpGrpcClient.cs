using ByteAwesome.SecretAPI.Grpc;
using ByteAwesome.UserAPI.Repository;

namespace ByteAwesome.UserAPI.GrpcClient
{
    public interface IOtpGrpcClient
    {
        Task<OtpProtoDto> Add(string emailOrPhoneNumber, OtpProtoType otpType, OtpActionProtoType actionType, string languageCode = "en");
        Task Verify(string emailOrPhoneNumber, string tacCode, OtpActionProtoType actionType, string languageCode = "en");
    }

    public class OtpGrpcClient : IOtpGrpcClient
    {
        private readonly OtpGrpcService.OtpGrpcServiceClient client;
        private readonly IUserRepository userRepository;

        public OtpGrpcClient(
            OtpGrpcService.OtpGrpcServiceClient client,
            IUserRepository userRepository
        )
        {
            this.client = client;
            this.userRepository = userRepository;
        }
        public async Task<OtpProtoDto> Add(string emailOrPhoneNumber, OtpProtoType otpType, OtpActionProtoType actionType, string languageCode = "en")
        {
            try
            {
                var request = new CreateOtpProtoDto
                {
                    Value = emailOrPhoneNumber,
                    Type = otpType,
                    ActionType = actionType,
                    LanguageCode = languageCode
                };
                var response = await client.AddAsync(request);
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
                return response;
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                throw new Exception("An error occurred while processing your request. Please try again later.");
            }
        }
        public async Task Verify(string emailOrPhoneNumber, string tacCode, OtpActionProtoType actionType, string languageCode = "en")
        {
            try
            {
                var user = await userRepository.GetUserByUserLoginIdentity(emailOrPhoneNumber);
                if (user is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                VerifyOTPProtoDto request = new VerifyOTPProtoDto
                {
                    UserId = user.Id.ToString(),
                    Value = emailOrPhoneNumber,
                    TacCode = tacCode,
                    ActionType = actionType,
                    LanguageCode = languageCode
                };
                var response = await client.VerifyAsync(request);
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                throw new Exception("An error occurred while processing your request. Please try again later.");
            }
        }
    }
}
