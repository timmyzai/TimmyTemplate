using AutoMapper;
using ByteAwesome.UserAPI.GrpcClient;
using ByteAwesome.SecretAPI.Grpc;
using ByteAwesome.UserAPI.Models.Dtos;
using ByteAwesome.UserAPI.Repository;
using Newtonsoft.Json;
using Serilog;

namespace ByteAwesome.UserAPI.Services
{
    public interface IUserServices
    {
        Task SendConfirmationEmail(UserDto user, HttpContext httpContext);
        Task SendForgotPasswordEmail(UserDto user, HttpContext httpContext);
        Task SendLoginOtpEmail(UserDto user, HttpContext httpContext);
        Task SendConfirmationPhoneSMS(UserDto user, HttpContext httpContext);
        Task<GenerateAccessTokenProtoResult> GenerateAccessToken(string userId, string userName, string email, string phoneNumber, IList<string> rolesNames, HttpContext httpContext);
    }

    public class UserServices : IUserServices
    {
        private readonly IOtpGrpcClient otpGrpcClient;
        private readonly INotisGrpcClient notisGrpcClient;
        private readonly IAccessTokenGrpcClient accessTokenGrpcClient;
        private readonly IUserLoginSessionInfoRepository userLoginSessionInfoRepository;
        private readonly IMapper mapper;

        public UserServices(
            IOtpGrpcClient otpGrpcClient,
            INotisGrpcClient notisGrpcClient,
            IAccessTokenGrpcClient accessTokenGrpcClient,
            IUserLoginSessionInfoRepository userLoginSessionInfoRepository,
            IMapper mapper
        )
        {
            this.otpGrpcClient = otpGrpcClient;
            this.notisGrpcClient = notisGrpcClient;
            this.accessTokenGrpcClient = accessTokenGrpcClient;
            this.userLoginSessionInfoRepository = userLoginSessionInfoRepository;
            this.mapper = mapper;
        }
        #region Email
        public async Task SendLoginOtpEmail(UserDto user, HttpContext httpContext)
        {
            var languageCode = CurrentSession.GetUserLanguage();
            var _otp = await otpGrpcClient.Add(user.UserData.EmailAddress, OtpProtoType.Email, OtpActionProtoType.Login, languageCode);

            var emailSubject = MessageContent.LoginOtpEmail;
            var emailContent = String.Format(LanguageService.Translate(MessageContent.OTPMessage), user.UserData.UserName, _otp.TacCode);
            _ = Task.Run(() => notisGrpcClient.SendEmail(user.UserData.DisplayName, user.UserData.EmailAddress, emailSubject, emailContent, languageCode));
        }
        public async Task SendConfirmationEmail(UserDto user, HttpContext httpContext)
        {
            var languageCode = CurrentSession.GetUserLanguage();
            if (user.UserData.IsEmailVerified)
            {
                throw new AppException(ErrorCodes.User.EmailAlreadyVerified);
            }
            var _otp = await otpGrpcClient.Add(user.UserData.EmailAddress, OtpProtoType.Email, OtpActionProtoType.EmailVerification, languageCode);

            var emailSubject = MessageContent.VerifyEmail;
            var emailContent = String.Format(LanguageService.Translate(MessageContent.OTPMessage), user.UserData.UserName, _otp.TacCode);
            _ = Task.Run(() => notisGrpcClient.SendEmail(user.UserData.DisplayName, user.UserData.EmailAddress, emailSubject, emailContent, languageCode));
        }
        public async Task SendForgotPasswordEmail(UserDto user, HttpContext httpContext)
        {
            var languageCode = CurrentSession.GetUserLanguage();
            var _otp = await otpGrpcClient.Add(user.UserData.EmailAddress, OtpProtoType.Email, OtpActionProtoType.ForgotPassword, languageCode);

            var emailSubject = MessageContent.PasswordResetEmail;
            var emailContent = String.Format(LanguageService.Translate(MessageContent.OTPMessage), user.UserData.UserName, _otp.TacCode);
            _ = Task.Run(() => notisGrpcClient.SendEmail(user.UserData.DisplayName, user.UserData.EmailAddress, emailSubject, emailContent, languageCode));
        }
        #endregion
        #region SMS
        public async Task SendConfirmationPhoneSMS(UserDto user, HttpContext httpContext)
        {
            if (string.IsNullOrEmpty(user.UserData.PhoneNumber))
            {
                throw new AppException(ErrorCodes.User.UserWithoutPhone);
            }
            if (user.UserData.IsPhoneVerified)
            {
                throw new AppException(ErrorCodes.User.PhoneAlreadyVerified);
            }
            var languageCode = CurrentSession.GetUserLanguage();
            var _otp = await otpGrpcClient.Add(user.UserData.PhoneNumber, OtpProtoType.Sms, OtpActionProtoType.PhoneVerification, languageCode);

            var smsSubject = LanguageService.Translate(MessageContent.VerifyPhoneNumber);
            var smsContent = String.Format(LanguageService.Translate(MessageContent.OTPMessage), user.UserData.UserName, _otp.TacCode);
            _ = Task.Run(() => notisGrpcClient.SendSMS(user.UserData.PhoneNumber, smsSubject, smsContent, languageCode));
        }
        #endregion
        #region Access Token
        public async Task<GenerateAccessTokenProtoResult> GenerateAccessToken(string userId, string userName, string email, string phoneNumber, IList<string> rolesNames, HttpContext httpContext)
        {
            #region Save User Device Info
            var deviceInfo = CurrentSession.GetUserDeviceInfo();
            var locationInfo = CurrentSession.GetUserLocationInfo();
            var userLoginSessionInfo = new UserLoginSessionInfoDto
            {
                UsersId = Guid.Parse(userId),
                LastLoginTime = DateTime.UtcNow
            };
            mapper.Map(deviceInfo, userLoginSessionInfo);
            var userCurrentLoginSessionIdString = CurrentSession.GetUserLoginSessionId();
            _ = Guid.TryParse(userCurrentLoginSessionIdString, out Guid userCurrentLoginSessionId);
            var existingSession = await userLoginSessionInfoRepository.GetById(userCurrentLoginSessionId);
            UserLoginSessionInfoDto userCurrentLoginSession;
            if (existingSession is not null)
            {
                Log.Information($"UserLoginSessionInfo Found, Updating UserLoginSessionInfo => {existingSession.Id}, UserId => {userId}, DeviceInfo => {JsonConvert.SerializeObject(deviceInfo)}, LocationInfo => {JsonConvert.SerializeObject(locationInfo)}");
                mapper.Map(locationInfo, existingSession);
                userCurrentLoginSession = await userLoginSessionInfoRepository.Update(existingSession);
            }
            else
            {
                mapper.Map(locationInfo, userLoginSessionInfo);
                userCurrentLoginSession = await userLoginSessionInfoRepository.Add(userLoginSessionInfo);
                Log.Information($"UserLoginSessionInfo Not Found, Adding UserLoginSessionInfo => {userCurrentLoginSession.Id}, UserId => {userId}, DeviceInfo => {JsonConvert.SerializeObject(deviceInfo)}, LocationInfo => {JsonConvert.SerializeObject(locationInfo)}");
                await userLoginSessionInfoRepository.CleanUpOldSessions();
            }
            #endregion
            var param = new GenerateAccessTokenProtoDto()
            {
                UserId = userId,
                UserName = userName,
                EmailAddress = email,
                PhoneNumber = phoneNumber,
                LanguageCode = CurrentSession.GetUserLanguage(),
                UserLoginSerssionId = userCurrentLoginSession.Id.ToString()
            };
            param.RoleNames.AddRange(rolesNames);
            var result = await accessTokenGrpcClient.GenerateAccessToken(param);
            return result;
        }
        #endregion
    }
}