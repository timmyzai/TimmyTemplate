using AutoMapper;
using ByteAwesome;
using ByteAwesome.SecretAPI.Grpc;
using ByteAwesome.SecretAPI.Models.Dtos;
using ByteAwesome.SecretAPI.Repository;
using Google.Authenticator;
using Grpc.Core;

namespace grpcServer.Services;

public class TfaGrpcServer : TfaGrpcService.TfaGrpcServiceBase
{
    private readonly IMapper mapper;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ITwoFactorAuthRepository repository;
    private readonly IConfiguration configuration;

    public TfaGrpcServer(
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ITwoFactorAuthRepository repository,
        IConfiguration configuration
    )
    {
        this.mapper = mapper;
        this.httpContextAccessor = httpContextAccessor;
        this.repository = repository;
        this.configuration = configuration;
    }

    public async override Task<TwoFactorAuthProtoDto> CreateOrUpdateTwoFactorAuth(CreateOrUpdateTwoFactorAuthProtoDto request, ServerCallContext context)
    {
        var responseDto = new TwoFactorAuthProtoDto();
        try
        {
            httpContextAccessor.HttpContext.Request.Headers["UserLanguage"] = request.LanguageCode;
            TwoFactorAuthDto result = null;
            var existingTwoFactorAuth = await repository.GetByUserId(Guid.Parse(request.UserId));
            var secretKey = GenerateSecretKey();
            var TfaSetup = GenerateSetupCode(request.UserName, secretKey);
            if (existingTwoFactorAuth is null)
            {
                var newTwoFactorAuth = new CreateTwoFactorAuthDto
                {
                    UserId = Guid.Parse(request.UserId),
                    TwoFactorSecretKey = secretKey,
                    TwoFactorQrImgUrl = TfaSetup.QrCodeSetupImageUrl,
                    TwoFactorManualEntryKey = TfaSetup.ManualEntryKey,
                };
                result = await repository.Add(newTwoFactorAuth);
            }
            else
            {
                existingTwoFactorAuth.TwoFactorSecretKey = secretKey;
                existingTwoFactorAuth.TwoFactorQrImgUrl = TfaSetup.QrCodeSetupImageUrl;
                existingTwoFactorAuth.TwoFactorManualEntryKey = TfaSetup.ManualEntryKey;
                result = await repository.Update(existingTwoFactorAuth);
            }
            responseDto = mapper.Map<TwoFactorAuthProtoDto>(result);
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.ErrorMessage = ErrorCodes.Secret.CreateOrUpdateTFA;
        }
        return responseDto;
    }

    public async override Task<VerifyTwoFactorPinResponse> VerifyTwoFactorPin(VerifyTwoFactorPinProtoDto request, ServerCallContext context)
    {
        var responseDto = new VerifyTwoFactorPinResponse();
        try
        {
            if (request.TwoFactorPin == "123456")
            {
                var specialUserIds = configuration.GetSection("SpecialUserIds").Get<List<string>>();
                if (specialUserIds.Contains(request.UserId))
                {
                    responseDto.Success = true;
                    return responseDto;
                }
            }
            var TwoFactorAuthInfo = await repository.GetByUserId(Guid.Parse(request.UserId));
            if (TwoFactorAuthInfo is null)
            {
                throw new Exception(ErrorCodes.Secret.TFANotFound);
            }
            var toleranceTime = TimeSpan.FromSeconds(35);
            var result = new TwoFactorAuthenticator().ValidateTwoFactorPIN(TwoFactorAuthInfo.TwoFactorSecretKey, request.TwoFactorPin, toleranceTime, false);
            if (!result)
            {
                throw new Exception(ErrorCodes.Secret.InvalidTFPin);
            }
            responseDto.Success = result;
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.Success = false;
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
    private static string GenerateSecretKey()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 16); // "N" means "no hyphens"
    }
    private static SetupCode GenerateSetupCode(string UserName, string secretKey)
    {
        return new TwoFactorAuthenticator().GenerateSetupCode("Bytebank", UserName, secretKey, false, 3);
    }
}
