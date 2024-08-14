using System.Text;
using System.Text.Json;
using ByteAwesome;
using ByteAwesome.SecretAPI.Grpc;
using ByteAwesome.SecretAPI.Models.Dtos;
using ByteAwesome.SecretAPI.Repository;
using ByteAwesome.Services;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Grpc.Core;

namespace grpcServer.Services;

public class PasskeyGrpcServer : PasskeyGrpcService.PasskeyGrpcServiceBase
{
    private readonly IFido2 fido2;
    private readonly IPasskeyRepository repository;
    private readonly IRedisCacheService redisCacheService;

    public PasskeyGrpcServer(
        IFido2 fido2,
        IPasskeyRepository repository,
        IRedisCacheService redisCacheService
    )
    {
        this.repository = repository;
        this.redisCacheService = redisCacheService;
        this.fido2 = fido2;
    }
    public async override Task<Create_ChallengeResponse> Create_Challenge(Create_ChallengeRequest request, ServerCallContext context)
    {
        var responseDto = new Create_ChallengeResponse();
        try
        {
            var existingCredentials = await repository.GetCredentialDescriptorsByUser(request.Username);
            CredentialCreateOptions options = fido2.RequestNewCredential(
                new Fido2User
                {
                    DisplayName = request.Username,
                    Name = request.Username,
                    Id = Encoding.UTF8.GetBytes(request.Username)
                },
                existingCredentials,
                new AuthenticatorSelection { UserVerification = UserVerificationRequirement.Required },
                request.AttestationType.ToEnum<AttestationConveyancePreference>()
            );
            await redisCacheService.SetAsync($"attestationOptions:{request.Username}", options.ToJson(), TimeSpan.FromMinutes(8));
            responseDto.CredentialOptionsJsonString = JsonSerializer.Serialize(options);
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
    public async override Task<CreateResponse> Create(CreateRequest request, ServerCallContext context)
    {
        var responseDto = new CreateResponse();
        try
        {
            var jsonOptions = await redisCacheService.GetAsync<string>($"attestationOptions:{request.Username}", true);
            var options = CredentialCreateOptions.FromJson(jsonOptions);
            await redisCacheService.DeleteAsync($"attestationOptions:{request.Username}");

            IsCredentialIdUniqueToUserAsyncDelegate callback = async (IsCredentialIdUniqueToUserParams args, CancellationToken ct) =>
            {
                var creds = await repository.GetCredentialById(args.CredentialId);
                return creds is null;
            };
            var attestationResponse = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(request.PendingCreateCredential);
            var success = await fido2.MakeNewCredentialAsync(attestationResponse, options, callback);
            if (success.Status != "ok")
            {
                throw new Exception(success.ErrorMessage);
            }

            var credential = new CreatePasskeyDto
            {
                CredentialId = success.Result.CredentialId,
                PublicKey = success.Result.PublicKey,
                UserHandle = success.Result.User.Id,
                SignatureCounter = success.Result.Counter,
                UserName = request.Username
            };
            await repository.Add(credential);
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
    public async override Task<Verify_ChallengeResponse> Verify_Challenge(Verify_ChallengeRequest request, ServerCallContext context)
    {
        var responseDto = new Verify_ChallengeResponse();
        try
        {
            var existingCredentials = await repository.GetCredentialDescriptorsByUser(request.Username);

            if (existingCredentials.Count == 0)
            {
                throw new Exception(ErrorCodes.User.PleaseCreatePassKey);
            }

            var options = fido2.GetAssertionOptions(existingCredentials, UserVerificationRequirement.Required);

            await redisCacheService.SetAsync($"attestationOptions:{request.Username}", options.ToJson(), TimeSpan.FromMinutes(8));

            responseDto.AssertionOptionsJsonString = JsonSerializer.Serialize(options);
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
    public async override Task<Verify_Response> Verify(Verify_Request request, ServerCallContext context)
    {
        var responseDto = new Verify_Response();
        try
        {
            var jsonOptions = await redisCacheService.GetAsync<string>($"attestationOptions:{request.Username}", true);
            var options = AssertionOptions.FromJson(jsonOptions);
            await redisCacheService.DeleteAsync($"attestationOptions:{request.Username}");
            
            var pendingVerifyCredential = JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(request.PendingVerifyCredential);

            var creds = await repository.GetCredentialById(pendingVerifyCredential.Id);
            if (creds is null)
            {
                throw new Exception(ErrorCodes.User.PassKeyNotRegisterOnPhone);
            }
            var storedCounter = creds.SignatureCounter;
            IsUserHandleOwnerOfCredentialIdAsync callback = async (args, ct) =>
            {
                var storedCreds = await repository.GetCredentialsByUserHandle(args.UserHandle);
                return storedCreds.Any(c => c.CredentialId.SequenceEqual(args.CredentialId));
            };

            var res = await fido2.MakeAssertionAsync(pendingVerifyCredential, options, creds.PublicKey, storedCounter, callback);
            if (res.Status != "ok")
            {
                throw new Exception(res.ErrorMessage);
            }
            creds.SignatureCounter = res.Counter;
            await repository.Update(creds);
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
    public async override Task<GetExistingPassKeyInfoResponse> GetExistingPassKeyInfo(GetExistingPassKeyInfoRequest request, ServerCallContext context)
    {
        var responseDto = new GetExistingPassKeyInfoResponse();
        try
        {
            var passkey = await repository.GetCredentialsByUser(request.UserName);
            responseDto.IsExist = passkey.Any();
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
}