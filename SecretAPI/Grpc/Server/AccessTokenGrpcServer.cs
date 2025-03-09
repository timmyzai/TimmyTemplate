using AutoMapper;
using AwesomeProject;
using SecretAPI.Helper;
using Grpc.Core;
using SecretAPI.Grpc;

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
    public async override Task<GenerateTokensAndPublicKeyProtoResult> GenerateTokensAndPublicKey(AuthenticationClaimsProtoDto request, ServerCallContext context)
    {
        var responseDto = new GenerateTokensAndPublicKeyProtoResult();
        try
        {
            var token = await jwtManagement.CreateTokenAsync(request);
            var refreshToken = jwtManagement.GenerateRefreshToken();
            responseDto.EncryptedAccessToken = token.EncryptedAccessToken;
            responseDto.ExpireInSeconds = token.ExpireInSeconds;
            responseDto.RefreshToken =  refreshToken.Token;
            responseDto.RefreshTokenExpireDate = refreshToken.ExpireDate.ToString();
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
    public async override Task<GenerateAccessTokenProtoResult> GenerateAccessToken(AuthenticationClaimsProtoDto request, ServerCallContext context)
    {
        var responseDto = new GenerateAccessTokenProtoResult();
        try
        {
            responseDto = await jwtManagement.CreateTokenAsync(request);
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
}

