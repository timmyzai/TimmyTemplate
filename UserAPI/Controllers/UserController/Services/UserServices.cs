using AutoMapper;
using UserAPI.GrpcClient;
using SecretAPI.Grpc;
using UserAPI.Models.Dtos;
using UserAPI.Repository;
using Services;
using AwesomeProject;
using UserAPI.Helper;

namespace UserAPI.Services
{
    public interface IUserServices
    {
        Task SendConfirmationEmail(UserDto user, bool isLogin = false);
        Task SendForgotPasswordEmail(UserDto user);
        Task SendLoginOtpEmail(UserDto user);
        Task<LoginResultDto> GenerateLoginResult(Guid userId, string userName, string email, string phoneNumber, IList<string> rolesNames);
        Task<RefreshTokenLoginResultDto> RefreshLoginToken(Guid userId, string userName, string email, string phoneNumber, IList<string> rolesNames);
    }

    public class UserServices : IUserServices
    {
        private readonly IMapper mapper;
        private readonly IOtpGrpcClient otpGrpcClient;
        private readonly IAccessTokenGrpcClient accessTokenGrpcClient;
        private readonly IRefreshTokenRepository refreshTokenRepository;
        private readonly IRedisCacheService redisCacheService;

        public UserServices(
            IMapper mapper,
            IOtpGrpcClient otpGrpcClient,
            IAccessTokenGrpcClient accessTokenGrpcClient,
            IRefreshTokenRepository refreshTokenRepository,
            IRedisCacheService redisCacheService
        )
        {
            this.mapper = mapper;
            this.otpGrpcClient = otpGrpcClient;
            this.accessTokenGrpcClient = accessTokenGrpcClient;
            this.refreshTokenRepository = refreshTokenRepository;
            this.redisCacheService = redisCacheService;
        }
        #region Email
        public async Task SendLoginOtpEmail(UserDto user)
        {
            await SendOtpEmail(user.UserData.DisplayName, user.UserData.EmailAddress, OtpActionProtoType.Login);
        }
        public async Task SendConfirmationEmail(UserDto user, bool isLogin = false)
        {
            if (user.UserData.IsEmailVerified)
            {
                throw new AppException(ErrorCodes.User.EmailAlreadyVerified);
            }
            await SendOtpEmail(user.UserData.DisplayName, user.UserData.EmailAddress, OtpActionProtoType.EmailVerification, isLogin);
        }
        public async Task SendForgotPasswordEmail(UserDto user)
        {
            await SendOtpEmail(user.UserData.DisplayName, user.UserData.EmailAddress, OtpActionProtoType.ForgotPassword);
        }
        #endregion
        #region User Login Session
        public async Task<LoginResultDto> GenerateLoginResult(Guid userId, string userName, string email, string phoneNumber, IList<string> rolesNames)
        {
            var accessTokenInfo = await accessTokenGrpcClient.GenerateTokensAndPublicKey(userId, userName, email, phoneNumber, rolesNames);
            await refreshTokenRepository.Add(new RefreshTokenDto()
            {
                UserId = userId,
                Token = accessTokenInfo.RefreshToken,
                ExpirationDate = DateTime.Parse(accessTokenInfo.RefreshTokenExpireDate)
            });
            return new LoginResultDto
            {
                EncryptedAccessToken = accessTokenInfo.EncryptedAccessToken,
                AccessTokenExpireDate = DateTime.UtcNow.AddSeconds(accessTokenInfo.ExpireInSeconds),
                RefreshToken = accessTokenInfo.RefreshToken,
                RefreshTokenExpireDate = DateTime.Parse(accessTokenInfo.RefreshTokenExpireDate)
            };
        }
        public async Task<RefreshTokenLoginResultDto> RefreshLoginToken(Guid userId, string userName, string email, string phoneNumber, IList<string> rolesNames)
        {
            var accessTokenInfo = await accessTokenGrpcClient.GenerateAccessToken(userId, userName, email, phoneNumber, rolesNames);
            return new RefreshTokenLoginResultDto
            {
                EncryptedAccessToken = accessTokenInfo.EncryptedAccessToken,
                AccessTokenExpireDate = DateTime.UtcNow.AddSeconds(accessTokenInfo.ExpireInSeconds)
            };
        }
        #endregion
        #region Private Methods
        private async Task SendOtpEmail(string receiverName, string emailOrPhoneNumber, OtpActionProtoType actionType, bool isLogin = false)
        {
            var otp = await otpGrpcClient.Add(emailOrPhoneNumber, OtpProtoType.Email, actionType, isLogin);
            if (string.IsNullOrEmpty(otp?.TacCode)) return;

            var emailSubject = actionType switch
            {
                OtpActionProtoType.EmailVerification => MessageContent.VerifyEmail,
                OtpActionProtoType.ForgotPassword => MessageContent.PasswordResetEmail,
                OtpActionProtoType.Login => MessageContent.LoginOtpEmail,
                _ => throw new Exception("Invalid action type")
            };
            var emailContent = string.Format(LanguageService.Translate(MessageContent.OTPMessage), emailOrPhoneNumber, otp.TacCode);
            var languageCode = CurrentSession.GetUserLanguage();
            var emailDto = new SendEmailDTO()
            {
                ReceiverName = receiverName,
                ReceiverEmail = emailOrPhoneNumber,
                Subject = emailSubject,
                Content = emailContent
            };
            _ = Task.Run(() => EmailHelper.SendEmail(emailDto));
        }
        #endregion
    }
}