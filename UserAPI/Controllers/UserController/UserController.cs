using Microsoft.AspNetCore.Mvc;
using UserAPI.Repository;
using UserAPI.Models.Dtos;
using UserAPI.Helper;
using Serilog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using UserAPI.Modules;
using UserAPI.GrpcClient;
using SecretAPI.Grpc;
using AutoMapper;
using UserAPI.Services;
using Services;
using UnitOfWork;
using UserAPI.DbContexts;
using System.ComponentModel.DataAnnotations;
using AwesomeProject;

namespace UserAPI.Controllers
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
        private readonly IUserServices userServices;
        private readonly IRedisCacheService redisCacheService;
        private readonly IRefreshTokenRepository refreshTokenRepository;
        private readonly ITokenService tokenService;

        public UserController(
            IUserRepository repository,
            IUserRolesRepository userRolesRepository,
            IRoleRepository roleRepository,
            IOptions<AppModuleConfig> appConfig,
            IOtpGrpcClient otpGrpcClient,
            IMapper mapper,
            ITfaGrpcClient tfaGrpcClient,
            IUnitOfWorkManager unitOfWorkManager,
            IUserServices userServices,
            IRedisCacheService redisCacheService,
            IRefreshTokenRepository refreshTokenRepository,
            ITokenService tokenService
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
            this.userServices = userServices;
            this.redisCacheService = redisCacheService;
            this.refreshTokenRepository = refreshTokenRepository;
            this.tokenService = tokenService;
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
                using (var uow = unitOfWorkManager.Begin<ApplicationDbContext>())
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
                try
                {
                    var isComingSoon = appConfig.Value.IsComingSoon;
                    if (!isComingSoon)
                    {
                        await userServices.SendConfirmationEmail(result);
                    }
                    displayMessage = "Registered successfully. Confirmation email will be sent.";
                }
                catch (Exception ex)
                {
                    displayMessage = "Registered successfully. Failed to send confirmation email.";
                    Log.Error(ex, displayMessage);
                }
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
                if (!user.UserData.RoleNames.Contains(RoleNames.Admin) && !user.UserData.IsEmailVerified)
                {
                    if (string.IsNullOrEmpty(input.EmailTacCode))
                    {
                        await userServices.SendConfirmationEmail(user, isLogin: true);
                        response.DisplayMessage = "Verify Otp Email will be sent.";
                        throw new AppException(ErrorCodes.User.RequireOtpToVerifyEmail, GeneralHelper.MaskEmail(user.UserData.EmailAddress));
                    }
                    await otpGrpcClient.Verify(user.Id, user.UserData.EmailAddress, input.EmailTacCode, OtpActionProtoType.EmailVerification);
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
                            await userServices.SendLoginOtpEmail(user);
                            response.DisplayMessage = "Login Otp Email will be sent.";
                            throw new AppException(ErrorCodes.User.RequireLoginOtp, GeneralHelper.MaskEmail(user.UserData.EmailAddress));
                        }
                        else
                        {
                            await otpGrpcClient.Verify(user.Id, user.UserData.EmailAddress, input.EmailTacCode, OtpActionProtoType.Login);
                        }
                    }
                }
                #endregion
                response.Result = await userServices.GenerateLoginResult(user.Id, user.UserData.UserName, user.UserData.EmailAddress, user.UserData.PhoneNumber, user.UserData.RoleNames);
                response.DisplayMessage = "Login Successfully";
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
                var currentUserId = CurrentSession.GetUser().Id;
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
        public async Task<ActionResult<ResponseDto<MaskedUserIdentityProfileDto>>> GetUserInfoByUserIdentity([Required] string userIdentity)
        {
            var response = new ResponseDto<MaskedUserIdentityProfileDto>();
            try
            {
                var result = await repository.GetUserByUserLoginIdentity(userIdentity);
                if (result is null || result.UserData.RoleNames.Contains("Admin"))
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                response.Result = new MaskedUserIdentityProfileDto(result);
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
                if (!PwdHashTokenHelper.VerifyHashedPassword(existingUser.UserData.PassWord, existingUser.UserData.PasswordSalt, input.OldPassword))
                {
                    throw new AppException(ErrorCodes.User.InvalidPassword);
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
                var currentUserId = CurrentSession.GetUser().Id;
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
                var currentUserId = CurrentSession.GetUser().Id;
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
                var currentUserId = CurrentSession.GetUser().Id;
                var existingUser = await repository.GetById(currentUserId);
                if (existingUser is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                if (existingUser.UserData.IsTwoFactorEnabled)
                {
                    throw new AppException(ErrorCodes.User.TwoFactorAlreadyEnabled);
                }
                var TwoFactorAuthInfo = await tfaGrpcClient.CreateOrUpdateTwoFactorAuth(existingUser.Id, existingUser.UserData.UserName);
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
                var currentUserId = CurrentSession.GetUser().Id;
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
                var currentUserId = CurrentSession.GetUser().Id;
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
        public async Task<ActionResult<ResponseDto<bool?>>> SendLoginOtpEmail([Required] string userLoginIdentity)
        {
            var response = new ResponseDto<bool?>();
            try
            {
                var user = await repository.GetUserByUserLoginIdentity(userLoginIdentity);
                if (user is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                await userServices.SendLoginOtpEmail(user);
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
        public async Task<ActionResult<ResponseDto<string>>> SendConfirmationEmail([Required] string userLoginIdentity)
        {
            var response = new ResponseDto<string>();
            try
            {
                var user = await repository.GetUserByUserLoginIdentity(userLoginIdentity);
                if (user is null)
                {
                    throw new AppException(ErrorCodes.User.UserNotExists);
                }
                await userServices.SendConfirmationEmail(user);
                response.DisplayMessage = "Confirmation Email will be sent.";
                response.Result = GeneralHelper.MaskEmail(user.UserData.EmailAddress); ;
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
        #endregion
        #region Forgot Password
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<string>>> SendForgotPasswordEmail([Required] string userLoginIdentity)
        {
            var response = new ResponseDto<string>();
            try
            {
                var user = await repository.GetUserByUserLoginIdentity(userLoginIdentity) ?? throw new AppException(ErrorCodes.User.UserNotExists);
                await userServices.SendForgotPasswordEmail(user);
                response.DisplayMessage = "Forgot password email will be sent.";
                response.Result = GeneralHelper.MaskEmail(user.UserData.EmailAddress);
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
                await otpGrpcClient.Verify(user.Id, user.UserData.EmailAddress, input.TacCode, OtpActionProtoType.ForgotPassword);

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
        #region RefreshSession
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<ResponseDto<RefreshTokenLoginResultDto>>> RefreshSession(RefreshSessionDto input)
        {
            var response = new ResponseDto<RefreshTokenLoginResultDto>();
            try
            {
                var userId = tokenService.GetUserIdFromExpiredToken(Request);
                var user = await repository.GetById(userId);
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
                var refreshToken = await refreshTokenRepository.GetByUserId(userId);
                if (refreshToken is null || refreshToken.Token != input.RefreshToken || refreshToken.ExpirationDate < DateTime.UtcNow)
                {
                    throw new AppException(ErrorCodes.General.PleaseLogin);
                }
                var result = await userServices.RefreshLoginToken(userId, user.UserData.UserName, user.UserData.EmailAddress, user.UserData.PhoneNumber, user.UserData.RoleNames);
                response.Result = result;
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

        #endregion
    }
}
