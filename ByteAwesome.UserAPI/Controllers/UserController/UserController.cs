using Microsoft.AspNetCore.Mvc;
using ByteAwesome.UserAPI.Repository;
using ByteAwesome.UserAPI.Models.Dtos;
using ByteAwesome.UserAPI.Helper;
using Serilog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using ByteAwesome.UserAPI.Modules;
using ByteAwesome.UserAPI.GrpcClient;
using ByteAwesome.SecretAPI.Grpc;
using AutoMapper;
using ByteAwesome.UserAPI.Helper.UnitOfWork;
using ByteAwesome.UserAPI.Services;
using ByteAwesome.Services;
using System.Text.Json;

namespace ByteAwesome.UserAPI.Controllers
{
    public class UserController : BaseController<UserDto, CreateUserDto, Guid, IUserRepository>
    {
        private readonly IUserRepository repository;
        private readonly IUserRolesRepository userRolesRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IOptions<AppModuleConfig> appConfig;
        private readonly IOtpGrpcClient otpGrpcClient;
        private readonly IMapper mapper;
        private readonly ITfaGrpcClient tfaGrpcClient;
        private readonly IUnitOfWorkManager unitOfWorkManager;
        private readonly IUserLoginSessionInfoRepository userLoginSessionInfoRepository;
        private readonly IKmsKeysGrpcClient kmsKeysGrpcClient;
        private readonly IUserServices userServices;
        private readonly IPasskeyGrpcClient passkeyGrpcClient;
        private readonly IRedisCacheService redisCacheService;
        private readonly IWalletGrpcClient walletGrpcClient;

        public UserController(
            IUserRepository repository,
            IUserRolesRepository userRolesRepository,
            IRoleRepository roleRepository,
            IOptions<AppModuleConfig> appConfig,
            IOtpGrpcClient otpGrpcClient,
            IMapper mapper,
            ITfaGrpcClient tfaGrpcClient,
            IUnitOfWorkManager unitOfWorkManager,
            IUserLoginSessionInfoRepository userLoginSessionInfoRepository,
            IKmsKeysGrpcClient kmsKeysGrpcClient,
            IUserServices userServices,
            IPasskeyGrpcClient passkeyGrpcClient,
            IRedisCacheService redisCacheService,
            IWalletGrpcClient walletGrpcClient
        ) : base(repository)
        {
            this.repository = repository;
            this.userRolesRepository = userRolesRepository;
            this.roleRepository = roleRepository;
            this.appConfig = appConfig;
            this.otpGrpcClient = otpGrpcClient;
            this.mapper = mapper;
            this.tfaGrpcClient = tfaGrpcClient;
            this.unitOfWorkManager = unitOfWorkManager;
            this.userLoginSessionInfoRepository = userLoginSessionInfoRepository;
            this.kmsKeysGrpcClient = kmsKeysGrpcClient;
            this.userServices = userServices;
            this.passkeyGrpcClient = passkeyGrpcClient;
            this.redisCacheService = redisCacheService;
            this.walletGrpcClient = walletGrpcClient;
        }

        #region Register and Login
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<EntityUserDto>>> Register([FromBody] RegisterDto input)
        {
            var response = new ResponseDto<EntityUserDto>();
            var result = new UserDto();
            try
            {
                var displayMessage = "Registering";
                using (var uow = unitOfWorkManager.Begin())
                {
                    #region Add User
                    await repository.CheckIfUserExists(input.UserName, input.PhoneNumber, input.EmailAddress);
                    result = await repository.Add(input);
                    #endregion

                    #region Add User Roles Junction Table
                    var role = await roleRepository.GetRoleByName(RoleNames.Clients);
                    var userRolesDto = new CreateUserRolesDto()
                    {
                        UsersId = result.Id,
                        RolesId = role.Id
                    };
                    await userRolesRepository.Add(userRolesDto);
                    #endregion
                    await uow.SaveChangesAsync();
                }
                _ = Task.Run(() => kmsKeysGrpcClient.CreateOrUpdateKmsKeys(result.Id));
                try
                {
                    var isComingSoon = appConfig.Value.IsComingSoon;
                    if (!isComingSoon)
                    {
                        await userServices.SendConfirmationEmail(result, HttpContext);
                    }
                    displayMessage = "Registered successfully. Confirmation email will be sent.";
                }
                catch (Exception ex)
                {
                    displayMessage = "Registered successfully. Failed to send confirmation email.";
                    Log.Error(ex, displayMessage);
                }
                #region Create Wallet Group
                _ = Task.Run(() => walletGrpcClient.CreateWalletGroup(result.Id, result.UserData.UserName, result.UserData.EmailAddress, result.UserData.PhoneNumber));
                #endregion
                response.Result = mapper.Map<EntityUserDto>(result);
                response.DisplayMessage = displayMessage;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response);
            }
            return Json(response);
        }
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<LoginResultDto>>> Login([FromBody] LoginDto input)
        {
            var response = new ResponseDto<LoginResultDto>();
            try
            {
                bool isComingSoon = appConfig.Value.IsComingSoon;
                if (isComingSoon)
                {
                    throw new AppException(ErrorCodes.General.ComingSoon);
                }
                var user = await repository.GetUserByUserLoginIdentity(input.UserLoginIdentityAddress);
                UserValidationClass.ValidateUser(user);
                #region Password Verification
                var isInvalidPassword = !PwdHashTokenHelper.VerifyHashedPassword(user.UserData.PassWord, user.UserData.PasswordSalt, input.Password);
                if (isInvalidPassword)
                {
                    user.UserData.PasswordTryCount++;
                    if (user.UserData.PasswordTryCount >= 50)
                    {
                        user.UserData.IsLockedOut = true;
                    }
                    await repository.Update(user);
                    throw new AppException(ErrorCodes.User.InvalidPassword);
                }
                if (user.UserData.PasswordTryCount > 0)
                {
                    user.UserData.PasswordTryCount = 0;
                    await repository.Update(user);
                }
                #endregion
                #region Email Verification and Two Factor Authentication Before Login
                var languageCode = CurrentSession.GetUserLanguage();
                if (!user.UserData.RoleNames.Contains(RoleNames.Admin) && !user.UserData.IsEmailVerified)
                {
                    if (string.IsNullOrEmpty(input.EmailTacCode))
                    {
                        await userServices.SendConfirmationEmail(user, HttpContext);
                        response.DisplayMessage = "Verify Otp Email will be sent.";
                        response.Result.User = new MaskedUserIdentityProfileDto(user);
                        throw new AppException(ErrorCodes.User.RequireOtpToVerifyEmail);
                    }
                    await otpGrpcClient.Verify(user.UserData.EmailAddress, input.EmailTacCode, OtpActionProtoType.EmailVerification, languageCode);
                    user.UserData.IsEmailVerified = true;
                    await repository.Update(user);
                }
                else if (user.UserData.IsTwoFactorEnabled)
                {
                    if (string.IsNullOrEmpty(input.TwoFactorPin))
                    {
                        throw new AppException(ErrorCodes.User.RequireTwoFactorPin);
                    }
                    await tfaGrpcClient.VerifyTwoFactorPin(user.Id, input.TwoFactorPin);
                }
                else
                {

                    if (!user.UserData.RoleNames.Contains(RoleNames.Admin))
                    {
                        if (string.IsNullOrEmpty(input.EmailTacCode))
                        {
                            await userServices.SendLoginOtpEmail(user, HttpContext);
                            response.DisplayMessage = "Login Otp Email will be sent.";
                            response.Result.User = new MaskedUserIdentityProfileDto(user);
                            throw new AppException(ErrorCodes.User.RequireLoginOtp);
                        }
                        else
                        {
                            await otpGrpcClient.Verify(user.UserData.EmailAddress, input.EmailTacCode, OtpActionProtoType.Login, languageCode);
                        }
                    }
                }
                #endregion
                #region Get Access Token From Secret GRPC
                var accessTokenInfo = await userServices.GenerateAccessToken(user.Id.ToString(), user.UserData.UserName, user.UserData.EmailAddress, user.UserData.PhoneNumber, user.UserData.RoleNames, HttpContext);
                #endregion
                #region Get Kms Public Key
                var getPublicKeyResult = await kmsKeysGrpcClient.GetPublicKey(user.Id);
                if (string.IsNullOrEmpty(getPublicKeyResult.PublicKey))
                {
                    var newKmsKeyResult = await kmsKeysGrpcClient.CreateOrUpdateKmsKeys(user.Id);
                    response.Result.PublicKey = newKmsKeyResult.PublicKey;
                }
                else
                {
                    response.Result.PublicKey = getPublicKeyResult.PublicKey;
                }
                #endregion
                response.Result.User = new MaskedUserIdentityProfileDto(user)
                {
                    UserId = user.Id
                };
                response.Result.EncryptedAccessToken = accessTokenInfo.EncryptedAccessToken;
                response.Result.ExpireInSeconds = accessTokenInfo.ExpireInSeconds;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.UserLoginFailed);
            }
            return Json(response);
        }
        #endregion
        #region Update User (TFA, Password, Profile)
        public async Task<ActionResult<ResponseDto<EntityUserDto>>> GetMyUserInfo()
        {
            var response = new ResponseDto<EntityUserDto>();
            try
            {
                var currentUserId = CurrentSession.GetUserId();
                var result = await repository.GetById(currentUserId);
                if (result is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                response.Result = mapper.Map<EntityUserDto>(result);
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.GetMyUserInfo);
            }
            return Json(response);
        }
        public async Task<ActionResult<ResponseDto<UserBasicInfoDto>>> GetUserInfoByUserIdentity(string userIdentity)
        {
            var response = new ResponseDto<UserBasicInfoDto>();
            try
            {
                var result = await repository.GetUserByUserLoginIdentity(userIdentity);
                if (result is null || result.UserData.RoleNames.Contains("Admin"))
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                response.Result = mapper.Map<UserBasicInfoDto>(result);
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.GetUserInfoByUserIdentity);
            }
            return Json(response);
        }
        public async Task<ActionResult<ResponseDto<EntityUserDto>>> ChangeUserPassword([FromBody] ChangeUserPasswordDto input)
        {
            var response = new ResponseDto<EntityUserDto>();
            try
            {
                var currentUser = CurrentSession.GetUser();
                var existingUser = await repository.GetById(currentUser.Id);
                if (existingUser is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                if (input.NewPassword != input.ConfirmPassword)
                {
                    throw new AppException(ErrorCodes.User.UserNewPasswordNotMatch);
                }
                await tfaGrpcClient.VerifyTwoFactorPin(currentUser.Id, input.TwoFactorPin);
                var (hash, salt) = PwdHashTokenHelper.CreateHash(input.NewPassword);
                existingUser.UserData.PassWord = hash;
                existingUser.UserData.PasswordSalt = salt;
                var result = await repository.Update(existingUser);
                response.Result = mapper.Map<EntityUserDto>(result);
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.ChangeUserPassword);
            }
            return Json(response);
        }
        public async Task<ActionResult<ResponseDto<EntityUserDto>>> UpdateUserProfile([FromBody] UpdateUserProfileDto input)
        {
            var response = new ResponseDto<EntityUserDto>();
            try
            {
                var currentUserId = CurrentSession.GetUserId();
                var existingUser = await repository.GetById(currentUserId);
                if (existingUser is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                existingUser.UserData.FirstName = input.FirstName;
                existingUser.UserData.LastName = input.LastName;

                var result = await repository.Update(existingUser);
                response.Result = mapper.Map<EntityUserDto>(result);
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.UpdateUserProfile);
            }
            return Json(response);
        }
        public async Task<ActionResult<ResponseDto<EntityUserDto>>> UpdateUserPhoneNumber([FromBody] UpdateUserPhoneNumberDto input)
        {
            var response = new ResponseDto<EntityUserDto>();
            try
            {
                var currentUserId = CurrentSession.GetUserId();
                var existingUser = await repository.GetById(currentUserId);
                if (existingUser is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                await tfaGrpcClient.VerifyTwoFactorPin(currentUserId, input.TwoFactorPin);
                existingUser.UserData.PhoneNumber = input.PhoneNumber;
                existingUser.UserData.IsPhoneVerified = false;

                var result = await repository.Update(existingUser);
                response.Result = mapper.Map<EntityUserDto>(result);
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.UpdateUserPhoneNumber);
            }
            return Json(response);
        }
        public async Task<ActionResult<ResponseDto<GetTwoFactorAuthInfoResult>>> GetTwoFactorAuthInfo()
        {
            var response = new ResponseDto<GetTwoFactorAuthInfoResult>();
            try
            {
                var currentUserId = CurrentSession.GetUserId();
                var existingUser = await repository.GetById(currentUserId);
                if (existingUser is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                if (existingUser.UserData.IsTwoFactorEnabled)
                {
                    throw new AppException(ErrorCodes.User.TwoFactorAlreadyEnabled);
                }
                var languageCode = CurrentSession.GetUserLanguage();
                var TwoFactorAuthInfo = await tfaGrpcClient.CreateOrUpdateTwoFactorAuth(existingUser.Id, existingUser.UserData.UserName, languageCode);
                response.Result.TwoFactorSecretKey = TwoFactorAuthInfo.TwoFactorSecretKey;
                response.Result.TwoFactorQrImgUrl = TwoFactorAuthInfo.TwoFactorQrImgUrl;
                response.Result.TwoFactorManualEntryKey = TwoFactorAuthInfo.TwoFactorManualEntryKey;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.GetTwoFactorAuthInfo);
            }
            return Json(response);
        }
        public async Task<ActionResult<ResponseDto<EntityUserDto>>> EnableOrDisableTwoFactorAuth([FromBody] EnableOrDisableTwoFactorAuthDto input)
        {
            var response = new ResponseDto<EntityUserDto>();
            try
            {
                var currentUserId = CurrentSession.GetUserId();
                var existingUser = await repository.GetById(currentUserId);
                if (existingUser is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                await tfaGrpcClient.VerifyTwoFactorPin(currentUserId, input.TwoFactorPin);
                existingUser.UserData.IsTwoFactorEnabled = input.Enable;
                var result = await repository.Update(existingUser);
                response.Result = mapper.Map<EntityUserDto>(result);
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.EnableOrDisableTwoFactorAuth);
            }
            return Json(response);
        }
        #endregion
        #region Delete User Account
        public async Task<ActionResult<ResponseDto<bool?>>> DeleteMyAccount([FromBody] DeleteMyAccountDto input)
        {
            var response = new ResponseDto<bool?>();
            try
            {
                var currentUserId = CurrentSession.GetUserId();
                var existingUser = await repository.GetById(currentUserId);
                if (existingUser is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                await tfaGrpcClient.VerifyTwoFactorPin(currentUserId, input.TwoFactorPin);
                await repository.Delete(existingUser.Id);
                response.Result = true;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.DeleteMyAccount);
            }
            return Json(response);
        }
        #endregion
        #region Confirmation
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<bool?>>> SendLoginOtpEmail(string emailAddress)
        {
            var response = new ResponseDto<bool?>();
            try
            {
                var user = await repository.GetUserByUserLoginIdentity(emailAddress);
                if (user is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                await userServices.SendLoginOtpEmail(user, HttpContext);
                response.DisplayMessage = "Login Otp Email will be sent.";
                response.Result = true;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.SendLoginOtpEmail);
            }
            return Json(response);
        }
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<bool?>>> SendConfirmationEmail(string emailAddress)
        {
            var response = new ResponseDto<bool?>();
            try
            {
                var user = await repository.GetUserByUserLoginIdentity(emailAddress);
                if (user is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                await userServices.SendConfirmationEmail(user, HttpContext);
                response.DisplayMessage = "Confirmation Email will be sent.";
                response.Result = true;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.SendConfirmationEmail);
            }
            return Json(response);
        }
        public async Task<ActionResult<ResponseDto<bool?>>> SendConfirmationPhoneSMS()
        {
            var response = new ResponseDto<bool?>();
            try
            {
                var currentUserId = CurrentSession.GetUserId();
                var existingUser = await repository.GetById(currentUserId);
                if (existingUser is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                await userServices.SendConfirmationPhoneSMS(existingUser, HttpContext);
                response.DisplayMessage = "Tac Code will be sent to " + existingUser.UserData.PhoneNumber;
                response.Result = true;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.SendConfirmationPhone);
            }
            return Json(response);
        }
        public async Task<ActionResult<ResponseDto<bool?>>> VerifyPhoneNumber([FromBody] VerifyPhoneNumberDto input)
        {
            var response = new ResponseDto<bool?>();
            try
            {
                var currentUserId = CurrentSession.GetUserId();
                var existingUser = await repository.GetById(currentUserId);
                if (existingUser is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                if (string.IsNullOrEmpty(existingUser.UserData.PhoneNumber))
                {
                    throw new AppException(ErrorCodes.User.UserWithoutPhone);
                }
                if (existingUser.UserData.IsPhoneVerified)
                {
                    throw new AppException(ErrorCodes.User.PhoneAlreadyVerified);
                }
                var languageCode = CurrentSession.GetUserLanguage();
                await otpGrpcClient.Verify(existingUser.UserData.PhoneNumber, input.TacCode, OtpActionProtoType.PhoneVerification, languageCode);

                existingUser.UserData.IsPhoneVerified = true;
                await repository.Update(existingUser);
                response.Result = true;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.VerifyPhoneNumber);
            }
            return Json(response);
        }
        #endregion
        #region Forgot Password
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<bool?>>> SendForgotPasswordEmail(string emailAddress)
        {
            var response = new ResponseDto<bool?>();
            try
            {
                var user = await repository.GetUserByUserLoginIdentity(emailAddress) ?? throw new AppException(ErrorCodes.User.UserNotExists);
                await userServices.SendForgotPasswordEmail(user, HttpContext);
                response.DisplayMessage = "Forgot password email will be sent.";
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.SendForgotPasswordEmail);
            }
            return Json(response);
        }
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<EntityUserDto>>> ChangeUserPasswordByEmail([FromBody] ChangeUserPasswordByEmailDto input)
        {
            var response = new ResponseDto<EntityUserDto>();
            try
            {
                var user = await repository.GetUserByUserLoginIdentity(input.Email);
                if (user is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                if (input.NewPassword != input.ConfirmPassword)
                {
                    throw new AppException(ErrorCodes.User.UserNewPasswordNotMatch);
                }
                var languageCode = CurrentSession.GetUserLanguage();
                await otpGrpcClient.Verify(input.Email, input.TacCode, OtpActionProtoType.ForgotPassword, languageCode);

                var (hash, salt) = PwdHashTokenHelper.CreateHash(input.NewPassword);
                user.UserData.PassWord = hash;
                user.UserData.PasswordSalt = salt;
                var result = await repository.Update(user);
                response.Result = mapper.Map<EntityUserDto>(result);
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.ChangeUserPasswordByEmail);
            }
            return Json(response);
        }
        #endregion
        #region UserLoginSession
        public async Task<ActionResult<ResponseDto<IEnumerable<UserLoginSessionInfoDto>>>> GetMyUserLoginSessions()
        {
            var response = new ResponseDto<IEnumerable<UserLoginSessionInfoDto>>();
            try
            {
                var currentUserId = CurrentSession.GetUserId();
                var items = await userLoginSessionInfoRepository.GetByUserId(currentUserId);
                response.Result = items;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.GetMyUserLoginSessions);
            }
            return Json(response);
        }
        [HttpGet]
        public async Task<ActionResult<ResponseDto<RefreshLoginTokenResultDto>>> RefreshSession()
        {
            var response = new ResponseDto<RefreshLoginTokenResultDto>();
            try
            {
                var UserId = CurrentSession.GetUserId();
                var user = await repository.GetById(UserId);
                if (user is null)
                {
                    throw new AppException(ErrorCodes.General.PleaseLogin);
                }
                if (!user.UserData.IsActive)
                {
                    throw new AppException(ErrorCodes.User.UserIsNotActive);
                }
                if (user.UserData.IsLockedOut)
                {
                    throw new AppException(ErrorCodes.User.UserLockedOut);
                }
                #region Get Access Token From Secret GRPC
                var accessTokenInfo = await userServices.GenerateAccessToken(user.Id.ToString(), user.UserData.UserName, user.UserData.EmailAddress, user.UserData.PhoneNumber, user.UserData.RoleNames, HttpContext);
                #endregion
                response.Result = new RefreshLoginTokenResultDto()
                {
                    EncryptedAccessToken = accessTokenInfo.EncryptedAccessToken,
                    ExpireInSeconds = accessTokenInfo.ExpireInSeconds
                };
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.General.PleaseLogin);
            }
            return Json(response);
        }
        public async Task<ActionResult<ResponseDto<bool?>>> LogOutAllSessions()
        {
            var response = new ResponseDto<bool?>();
            try
            {
                var currentUserId = CurrentSession.GetUserId();
                var user = await repository.GetById(currentUserId);
                if (user is null)
                {
                    throw new AppException(ErrorCodes.General.PleaseLogin);
                }
                var allUserSessions = await userLoginSessionInfoRepository.GetByUserId(currentUserId);
                foreach (var session in allUserSessions)
                {
                    await redisCacheService.DeleteAsync(session.Id.ToString());
                }
                var deletedSessionIds = allUserSessions.Select(x => x.Id).ToList();
                await userLoginSessionInfoRepository.DeleteRange(deletedSessionIds);
                response.Result = true;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.UserLogoutFailed);
            }
            return Json(response);
        }
        [AllowAnonymous] //allow expired token to logout
        public async Task<ActionResult<ResponseDto<bool?>>> LogOut([FromBody] LogOutDto input = null)
        {
            var response = new ResponseDto<bool?>();
            try
            {
                var userId = CurrentSession.GetUserId(); // must pass in token
                var user = await repository.GetById(userId);
                if (user is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }

                if (input is not null && input.UserLoginSessionList is not null && input.UserLoginSessionList.Count > 0)
                {
                    foreach (var session in input.UserLoginSessionList)
                    {
                        var deletedSession = await userLoginSessionInfoRepository.Delete(session.Id);
                        await redisCacheService.DeleteAsync(deletedSession.Id.ToString());
                    }
                }
                else
                {
                    var userCurrentLoginSessionIdString = CurrentSession.GetUserLoginSessionId();
                    _ = Guid.TryParse(userCurrentLoginSessionIdString, out Guid userCurrentLoginSessionId);
                    if (userCurrentLoginSessionId != Guid.Empty)
                    {
                        var userCurrentLoginSession = await userLoginSessionInfoRepository.GetById(userCurrentLoginSessionId);
                        if (userCurrentLoginSession is not null)
                        {
                            await userLoginSessionInfoRepository.Delete(userCurrentLoginSession.Id);
                            await redisCacheService.DeleteAsync(userCurrentLoginSession.Id.ToString());
                        }
                    }
                }
                await userLoginSessionInfoRepository.CleanUpOldSessions();
                response.Result = true;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.EnableOrDisableTwoFactorAuth);
            }
            return Json(response);
        }
        #endregion
        #region Passwordless Login
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<LoginResultDto>>> LoginWithPassKey([FromBody] LoginWithPassKeyDto input)
        {
            var response = new ResponseDto<LoginResultDto>();
            try
            {
                var user = await repository.GetUserByUserLoginIdentity(input.UserLoginIdentityAddress);
                UserValidationClass.ValidateUser(user);
                if (!user.UserData.IsPassKeyEnabled)
                {
                    throw new AppException(ErrorCodes.User.PassKeyNotEnabled);
                }
                if (input.PendingVerifyCredential is null)
                {
                    throw new AppException(ErrorCodes.User.PendingVerifyCredential);
                }
                await passkeyGrpcClient.Verify(user.UserData.UserName, JsonSerializer.Serialize(input.PendingVerifyCredential));

                #region Get Access Token From Secret GRPC
                var accessTokenInfo = await userServices.GenerateAccessToken(user.Id.ToString(), user.UserData.UserName, user.UserData.EmailAddress, user.UserData.PhoneNumber, user.UserData.RoleNames, HttpContext);
                #endregion
                #region Get Kms Public Key
                var getPublicKeyResult = await kmsKeysGrpcClient.GetPublicKey(user.Id);
                if (string.IsNullOrEmpty(getPublicKeyResult.PublicKey))
                {
                    var newKmsKeyResult = await kmsKeysGrpcClient.CreateOrUpdateKmsKeys(user.Id);
                    response.Result.PublicKey = newKmsKeyResult.PublicKey;
                }
                else
                {
                    response.Result.PublicKey = getPublicKeyResult.PublicKey;
                }
                #endregion
                response.Result.User = new MaskedUserIdentityProfileDto(user);
                response.Result.User.UserId = user.Id;
                response.Result.EncryptedAccessToken = accessTokenInfo.EncryptedAccessToken;
                response.Result.ExpireInSeconds = accessTokenInfo.ExpireInSeconds;
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.UserLoginFailed);
            }
            return Json(response);
        }
        public async Task<ActionResult<ResponseDto<UserDto>>> EnableDisablePassKey([FromBody] EnableDisablePassKeyDto input)
        {
            var response = new ResponseDto<UserDto>();
            try
            {
                var username = CurrentSession.GetUserName();
                var user = await repository.GetUserByUserLoginIdentity(username);
                UserValidationClass.ValidateUser(user);
                if (user.UserData.IsPassKeyEnabled == input.IsEnable)
                {
                    throw new AppException(input.IsEnable ? ErrorCodes.User.PassKeyAlreadyEnabled : ErrorCodes.User.PassKeyAlreadyDisabled);
                }
                if (input.IsEnable)
                {
                    var userHasExistingPassKey = await passkeyGrpcClient.UserHasExistingPassKey(user.UserData.UserName);
                    if (!userHasExistingPassKey)
                    {
                        throw new AppException(ErrorCodes.User.PleaseCreatePassKey);
                    }
                }
                if (input.PendingVerifyCredential is null)
                {
                    throw new AppException(ErrorCodes.User.PendingVerifyCredential);
                }
                await passkeyGrpcClient.Verify(user.UserData.UserName, JsonSerializer.Serialize(input.PendingVerifyCredential));

                user.UserData.IsPassKeyEnabled = input.IsEnable;
                response.Result = await repository.Update(user);
                response.DisplayMessage = $"Passkey Credential {(input.IsEnable ? "Enabled" : "Disabled")} Successfully.";
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, statusCode: ErrorCodes.User.EnableDisablePassKeyError);
            }
            return Json(response);
        }
        #endregion
    }
}
