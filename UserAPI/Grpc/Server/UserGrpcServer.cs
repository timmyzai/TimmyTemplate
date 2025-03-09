using Grpc.Core;
using UserAPI.Repository;
using UserAPI.Grpc;
using AwesomeProject;

namespace grpcServer.Services;

public class UserGrpcServer : UserGrpcService.UserGrpcServiceBase
{
    private readonly IUserRepository userRepository;

    public UserGrpcServer(
        IUserRepository userRepository
    )
    {
        this.userRepository = userRepository;
    }
    public async override Task<GetUserResultResponse> GetUserById(GetUserByIdProtoDto request, ServerCallContext context)
    {
        var responseDto = new GetUserResultResponse();
        try
        {
            var userId = Guid.Parse(request.UserId);
            var user = await userRepository.GetById(userId);
            if (user is null)
            {
                throw new Exception(ErrorCodes.User.UserNotExists);
            }
            responseDto.UserId = user.Id.ToString();
            responseDto.EmailAddress = user.UserData.EmailAddress;
            responseDto.PhoneNumber = user.UserData.PhoneNumber;
            responseDto.Username = user.UserData.UserName;
            responseDto.Success = true;
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
    public async override Task<GetUserResultResponse> GetUserByIdentity(GetUserByIdentityProtoDto request, ServerCallContext context)
    {
        var responseDto = new GetUserResultResponse();
        try
        {
            var user = await userRepository.GetUserByUserLoginIdentity(request.UserIdentity);
            if (user is null)
            {
                throw new Exception(ErrorCodes.User.UserNotExists);
            }
            responseDto.UserId = user.Id.ToString();
            responseDto.EmailAddress = user.UserData.EmailAddress;
            responseDto.PhoneNumber = user.UserData.PhoneNumber;
            responseDto.Username = user.UserData.UserName;
            responseDto.Success = true;
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
}
