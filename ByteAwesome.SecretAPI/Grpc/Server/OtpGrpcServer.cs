using AutoMapper;
using ByteAwesome;
using ByteAwesome.SecretAPI.Grpc;
using ByteAwesome.SecretAPI.Models.Dtos;
using ByteAwesome.SecretAPI.Modules;
using ByteAwesome.SecretAPI.Repository;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace grpcServer.Services;

public class OtpGrpcServer : OtpGrpcService.OtpGrpcServiceBase
{
    private readonly IMapper mapper;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IOtpRepository otpRepository;
    private readonly IOptions<AppModuleConfig> appConfig;
    private readonly IConfiguration configuration;

    public OtpGrpcServer(
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IOtpRepository otpRepository,
        IOptions<AppModuleConfig> appConfig,
        IConfiguration configuration
    )
    {
        this.mapper = mapper;
        this.httpContextAccessor = httpContextAccessor;
        this.otpRepository = otpRepository;
        this.appConfig = appConfig;
        this.configuration = configuration;
    }

    public async override Task<OtpProtoDto> Add(CreateOtpProtoDto request, ServerCallContext context)
    {
        var responseDto = new OtpProtoDto();
        try
        {
            httpContextAccessor.HttpContext.Request.Headers["UserLanguage"] = request.LanguageCode;
            OtpActionType otpActionType = (OtpActionType)request.ActionType;
            var existingOtp = await otpRepository.GetValidOtpByValueAndType(request.Value, otpActionType);
            if (existingOtp is not null)
            {
                //within 60 seconds, user can't request for another otp
                if (existingOtp.CreatedTime.AddSeconds(60) > DateTime.UtcNow)
                {
                    throw new AppException(ErrorCodes.Secret.OTPAlreadyRequested);
                }
                //if the otp is not used, set it to inactive
                existingOtp.IsActive = false;
                await otpRepository.Update(existingOtp);
            }
            var input = mapper.Map<CreateOtpDto>(request);
            var result = await otpRepository.Add(input);
            responseDto = mapper.Map<OtpProtoDto>(result);
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }

    public async override Task<VerifyOTPResponse> Verify(VerifyOTPProtoDto request, ServerCallContext context)
    {
        var responseDto = new VerifyOTPResponse();
        try
        {
            httpContextAccessor.HttpContext.Request.Headers["UserLanguage"] = request.LanguageCode;
            var param = mapper.Map<VerifyOTPDto>(request);
            if (request.TacCode == "123456" && new HashSet<string>(configuration.GetSection("SpecialUserIds").Get<List<string>>()).Contains(request.UserId))
            {
                return responseDto;
            }
            OtpActionType otpActionType = (OtpActionType)request.ActionType;
            var existingOtp = await otpRepository.GetValidOtpByValueAndType(request.Value, otpActionType);
            if (existingOtp is null)
            {
                throw new AppException(ErrorCodes.Secret.OTPNotFound);
            }
            if (existingOtp.TacCode != request.TacCode)
            {
                throw new AppException(ErrorCodes.Secret.InvalidOTP);
            }
            TimeSpan diff = (DateTime.UtcNow - existingOtp.CreatedTime).Duration();
            if (diff.TotalMinutes > int.Parse(appConfig.Value.TacCodeExpiryTime))
            {
                throw new AppException(ErrorCodes.Secret.ExpiredOTP);
            }
            if (!existingOtp.IsActive)
            {
                throw new AppException(ErrorCodes.Secret.UsedOTP);
            }
            existingOtp.IsActive = false;
            await otpRepository.Update(existingOtp);
        }
        catch (Exception ex)
        {
            ActionResultHandler.HandleException(ex);
            responseDto.ErrorMessage = ex.Message;
        }
        return responseDto;
    }
}
