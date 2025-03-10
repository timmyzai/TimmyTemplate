using AutoMapper;
using AwesomeProject;
using SecretAPI.Models.Dtos;
using SecretAPI.Modules;
using SecretAPI.Repository;
using Google.Authenticator;
using Grpc.Core;
using SecretAPI.Grpc;

namespace grpcServer.Services;

public class TfaGrpcServer : TfaGrpcService.TfaGrpcServiceBase
{
    private readonly IMapper mapper;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ITwoFactorAuthRepository repository;
    private readonly SpecialUserModuleConfig specialUserModuleConfig;

    public TfaGrpcServer(
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ITwoFactorAuthRepository repository,
        SpecialUserModuleConfig specialUserModuleConfig
    )
    {
        this.mapper = mapper;
        this.httpContextAccessor = httpContextAccessor;
        this.repository = repository;
        this.specialUserModuleConfig = specialUserModuleConfig;
    }

    public async override Task<TwoFactorAuthProtoDto> CreateOrUpdateTwoFactorAuth(CreateOrUpdateTwoFactorAuthProtoDto request, ServerCallContext context)
    {
        var responseDto = new TwoFactorAuthProtoDto();
        try
        {
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
                if (specialUserModuleConfig.SpecialUserIds.Contains(request.UserId))
                {
                    return responseDto;
                }
            }
            var TwoFactorAuthInfo = await repository.GetByUserId(Guid.Parse(request.UserId));
            if (TwoFactorAuthInfo is null)
            {
                throw new Exception(ErrorCodes.Secret.TFANotFound);
            }
            var toleranceTime = TimeSpan.FromSeconds(35);
            var isValid = new TwoFactorAuthenticator().ValidateTwoFactorPIN(TwoFactorAuthInfo.TwoFactorSecretKey, request.TwoFactorPin, toleranceTime, false);
            if (!isValid)
            {
                throw new Exception(ErrorCodes.Secret.InvalidTFPin);
            }
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
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
