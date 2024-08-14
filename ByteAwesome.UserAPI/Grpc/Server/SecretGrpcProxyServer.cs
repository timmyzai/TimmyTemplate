using Grpc.Core;
using ByteAwesome.UserAPI.Repository;
using ByteAwesome.UserAPI.Grpc;
using ByteAwesome;
using ByteAwesome.UserAPI.GrpcClient;

namespace grpcServer.Services;

public class SecretGrpcProxyServer : SecretGrpcProxyService.SecretGrpcProxyServiceBase
{
    private readonly IUserRepository userRepository;
    private readonly IPasskeyGrpcClient passkeyGrpcClient;
    private readonly ITfaGrpcClient tfaGrpcClient;
    private readonly IKmsKeysGrpcClient kmsKeysGrpcClient;

    public SecretGrpcProxyServer(
        IUserRepository userRepository,
        IPasskeyGrpcClient passkeyGrpcClient,
        ITfaGrpcClient tfaGrpcClient,
        IKmsKeysGrpcClient kmsKeysGrpcClient
    )
    {
        this.userRepository = userRepository;
        this.passkeyGrpcClient = passkeyGrpcClient;
        this.tfaGrpcClient = tfaGrpcClient;
        this.kmsKeysGrpcClient = kmsKeysGrpcClient;
    }
    public async override Task<VerifyPassKeyResponse> VerifyPassKey(VerifyPassKeyRequest request, ServerCallContext context)
    {
        var responseDto = new VerifyPassKeyResponse();
        try
        {
            if (!Guid.TryParse(request.UserId, out Guid userId)) throw new ArgumentException("Invalid user ID format.");
            var user = await userRepository.GetById(userId);
            if (user is null)
            {
                throw new Exception(ErrorCodes.User.UserNotExists);
            }
            if (!user.UserData.IsPassKeyEnabled)
            {
                throw new Exception(ErrorCodes.User.PassKeyNotEnabled);
            }
            await passkeyGrpcClient.Verify(user.UserData.UserName, request.PendingVerifyCredential);
            responseDto.Success = true;
        }
        catch (Exception ex)
        {
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
    public async override Task<VerifyTwoFactorPinResponse> VerifyTwoFactorPin(VerifyTwoFactorPinRequest request, ServerCallContext context)
    {
        var responseDto = new VerifyTwoFactorPinResponse();
        try
        {
            if (!Guid.TryParse(request.UserId, out Guid userId)) throw new ArgumentException("Invalid user ID format.");
            var user = await userRepository.GetById(userId);
            if (user is null)
            {
                throw new Exception(ErrorCodes.User.UserNotExists);
            }
            if (!user.UserData.IsTwoFactorEnabled && !user.UserData.RoleNames.Contains("Admin"))
            {
                throw new Exception(ErrorCodes.User.PleaseEnableTwoFactorAuth);
            }
            await tfaGrpcClient.VerifyTwoFactorPin(user.Id, request.TwoFactorPin);
            responseDto.Success = true;
        }
        catch (Exception ex)
        {
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
    public async override Task<VerifyDataAndDecryptResponse> VerifyDataAndDecrypt(VerifyDataAndDecryptRequest request, ServerCallContext context)
    {
        var responseDto = new VerifyDataAndDecryptResponse();
        try
        {
            if (!Guid.TryParse(request.UserId, out Guid userId)) throw new ArgumentException("Invalid user ID format.");
            var user = await userRepository.GetById(userId);
            if (user is null)
            {
                throw new Exception(ErrorCodes.User.UserNotExists);
            }
            var kmsResponse = await kmsKeysGrpcClient.VerifyDataAndDecrypt(user.Id, request.EncryptedData);
            responseDto.DecryptedData = kmsResponse.DecryptedData;
            responseDto.Success = true;
        }
        catch (Exception ex)
        {
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
}