using AutoMapper;
using ByteAwesome;
using ByteAwesome.SecretAPI.Grpc;
using ByteAwesome.SecretAPI.Helper;
using Grpc.Core;

namespace grpcServer.Services;

public class AccessTokenGrpcServer : AccessTokenGrpcService.AccessTokenGrpcServiceBase
{
    private readonly IMapper mapper;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IJwtManagement jwtManagement;

    public AccessTokenGrpcServer(
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IJwtManagement jwtManagement
    )
    {
        this.mapper = mapper;
        this.httpContextAccessor = httpContextAccessor;
        this.jwtManagement = jwtManagement;
    }
    public async override Task<GenerateAccessTokenProtoResult> GenerateAccessToken(GenerateAccessTokenProtoDto request, ServerCallContext context)
    {
        var responseDto = new GenerateAccessTokenProtoResult();
        try
        {
            httpContextAccessor.HttpContext.Request.Headers["UserLanguage"] = request.LanguageCode;
            var result = await jwtManagement.CreateTokenAsync(request);
            responseDto = mapper.Map<GenerateAccessTokenProtoResult>(result);
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
}
