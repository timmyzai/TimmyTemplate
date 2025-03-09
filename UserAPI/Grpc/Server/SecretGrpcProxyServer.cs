using Grpc.Core;
using UserAPI.Repository;
using UserAPI.Grpc;
using AwesomeProject;
using UserAPI.GrpcClient;

namespace grpcServer.Services;

public class SecretGrpcProxyServer : SecretGrpcProxyService.SecretGrpcProxyServiceBase
{
    private readonly IUserRepository userRepository;
    private readonly ITfaGrpcClient tfaGrpcClient;

    public SecretGrpcProxyServer(
        IUserRepository userRepository,
        ITfaGrpcClient tfaGrpcClient
    )
    {
        this.userRepository = userRepository;
        this.tfaGrpcClient = tfaGrpcClient;
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
            var proxyResponse = await tfaGrpcClient.VerifyTwoFactorPin(user.Id, request.TwoFactorPin, isProxy: true);
            responseDto.ErrorMessage = proxyResponse.ErrorMessage;
        }
        catch (Exception ex)
        {
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
}