using System.Text.Json;
using ByteAwesome.UserAPI.GrpcClient;
using ByteAwesome.UserAPI.Models.Dtos;
using ByteAwesome.UserAPI.Repository;
using ByteAwesome.UserAPI.Services;
using Fido2NetLib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome.UserAPI.Controllers
{
    public class PassKeyController : BaseController
    {
        private readonly IUserRepository userRepository;
        private readonly IPasskeyGrpcClient passkeyGrpcClient;
        private readonly ITfaGrpcClient tfaGrpcClient;

        public PassKeyController(
            IUserRepository userRepository,
            IPasskeyGrpcClient passkeyGrpcClient,
            ITfaGrpcClient tfaGrpcClient
        )
        {
            this.userRepository = userRepository;
            this.passkeyGrpcClient = passkeyGrpcClient;
            this.tfaGrpcClient = tfaGrpcClient;
        }
        #region Create
        public async Task<ActionResult<ResponseDto<CreatePassKeyDto>>> Create([FromBody] CreatePassKeyDto input)
        {
            var response = new ResponseDto<CreatePassKeyDto>();
            try
            {
                var userId = CurrentSession.GetUserId();
                var user = await userRepository.GetById(userId);
                if (user is null)
                {
                    throw new AppException(ErrorCodes.General.PleaseLogin);
                }
                await tfaGrpcClient.VerifyTwoFactorPin(user.Id, input.TwoFactorPin);

                await passkeyGrpcClient.Create(user.UserData.UserName, JsonSerializer.Serialize(input.PendingCreateCredential));

                user.UserData.IsPassKeyEnabled = true;
                await userRepository.Update(user);

                response.DisplayMessage = "Passkey Credential Created Successfully.";
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
        #endregion
        #region Get Info
        public async Task<ActionResult<ResponseDto<bool?>>> GetUserHasExistingPassKeyBool()
        {
            var response = new ResponseDto<bool?>();
            try
            {
                var userId = CurrentSession.GetUserId();
                var user = await userRepository.GetById(userId);
                UserValidationClass.ValidateUser(user);
                var userHasExistingPassKey = await passkeyGrpcClient.UserHasExistingPassKey(user.UserData.UserName);
                response.Result = userHasExistingPassKey;
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
        #endregion
        #region Challenges
        /// <summary>
        /// Create challenge to create a new passkey credential to call passkey Create API
        /// </summary>
        public async Task<ActionResult<ResponseDto<CredentialCreateOptions>>> GetChallengeToCreate()
        {
            var response = new ResponseDto<CredentialCreateOptions>();
            try
            {
                var userId = CurrentSession.GetUserId();
                var user = await userRepository.GetById(userId);
                if (user is null)
                {
                    throw new AppException(ErrorCodes.General.PleaseLogin);
                }
                var result = await passkeyGrpcClient.GetCreateCredentialChallenge(user.UserData.UserName);
                response.Result = result;
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
        /// <summary>
        /// Create challenge to find existing passkey credential for API requires passkey verification
        /// </summary>
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<AssertionOptions>>> GetChallengeToLogin([FromQuery] LoginIdentityDto input)
        {
            var response = new ResponseDto<AssertionOptions>();
            try
            {
                var user = await userRepository.GetUserByUserLoginIdentity(input.UserLoginIdentityAddress);
                UserValidationClass.ValidateUser(user);
                if (!user.UserData.IsPassKeyEnabled)
                {
                    throw new AppException(ErrorCodes.User.PassKeyNotEnabled);
                }
                var result = await passkeyGrpcClient.GetRetrieveCredentialChallenge(user.UserData.UserName);
                response.Result = result;
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
        public async Task<ActionResult<ResponseDto<AssertionOptions>>> GetChallengeToVerify()
        {
            var response = new ResponseDto<AssertionOptions>();
            try
            {
                var userId = CurrentSession.GetUserId();
                var user = await userRepository.GetById(userId);
                UserValidationClass.ValidateUser(user);
                var result = await passkeyGrpcClient.GetRetrieveCredentialChallenge(user.UserData.UserName);
                response.Result = result;
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
        #endregion
    }
}