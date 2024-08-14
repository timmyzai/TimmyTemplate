using System.Text.Json;
using ByteAwesome.SecretAPI.Grpc;
using Fido2NetLib;
using Fido2NetLib.Objects;

namespace ByteAwesome.UserAPI.GrpcClient
{
    public interface IPasskeyGrpcClient
    {
        Task Create(string userName, string pendingCreateCredential);
        Task<CredentialCreateOptions> GetCreateCredentialChallenge(string userName);
        Task<AssertionOptions> GetRetrieveCredentialChallenge(string userName);
        Task Verify(string userName, string verifyPassKeyChallengeResult);
        Task<bool> UserHasExistingPassKey(string userName);
    }

    public class PasskeyGrpcClient : IPasskeyGrpcClient
    {
        private readonly PasskeyGrpcService.PasskeyGrpcServiceClient client;

        public PasskeyGrpcClient(PasskeyGrpcService.PasskeyGrpcServiceClient client)
        {
            this.client = client;
        }
        public async Task<CredentialCreateOptions> GetCreateCredentialChallenge(string userName)
        {
            try
            {
                var param = new Create_ChallengeRequest
                {
                    Username = userName,
                    AttestationType = AttestationConveyancePreference.Direct.ToString()
                };
                var response = await client.Create_ChallengeAsync(param);
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
                var result = JsonSerializer.Deserialize<CredentialCreateOptions>(response.CredentialOptionsJsonString);
                return result;
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                throw new Exception("An error occurred while processing your request. Please try again later.");
            }
        }
        public async Task Create(string userName, string pendingCreateCredential)
        {
            try
            {
                PasskeyValidatorHelper.ValidateAttetation(pendingCreateCredential);
                var param = new CreateRequest
                {
                    Username = userName,
                    PendingCreateCredential = pendingCreateCredential
                };
                var response = await client.CreateAsync(param);
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                throw new Exception("An error occurred while processing your request. Please try again later.");
            }
        }
        public async Task<AssertionOptions> GetRetrieveCredentialChallenge(string userName)
        {
            try
            {
                var response = new Verify_ChallengeResponse();
                try
                {
                    var param = new Verify_ChallengeRequest
                    {
                        Username = userName
                    };
                    response = await client.Verify_ChallengeAsync(param);
                }
                catch (Exception ex)
                {
                    ActionResultHandler.HandleException(ex);
                    response.ErrorMessage = ex.Message;
                }
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
                return JsonSerializer.Deserialize<AssertionOptions>(response.AssertionOptionsJsonString);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                throw new Exception("An error occurred while processing your request. Please try again later.");
            }
        }
        public async Task Verify(string userName, string verifyPassKeyChallengeResult)
        {
            try
            {
                PasskeyValidatorHelper.ValidateAssertion(verifyPassKeyChallengeResult);
                var param = new Verify_Request
                {
                    Username = userName,
                    PendingVerifyCredential = verifyPassKeyChallengeResult
                };
                var response = await client.VerifyAsync(param);
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                throw new Exception("An error occurred while processing your request. Please try again later.");
            }
        }
        public async Task<bool> UserHasExistingPassKey(string userName)
        {
            try
            {
                var param = new GetExistingPassKeyInfoRequest
                {
                    UserName = userName
                };
                var response = await client.GetExistingPassKeyInfoAsync(param);
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage);
                return response.IsExist;
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                throw new Exception("An error occurred while processing your request. Please try again later.");
            }
        }
    }
}