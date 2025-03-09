using AutoMapper;
using SecretAPI.Grpc;
using SecretAPI.Models;
using SecretAPI.Models.Dtos;

namespace SecretAPI
{
    public static class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<TwoFactorAuthDto, TwoFactorAuth>().ReverseMap();
                config.CreateMap<CreateTwoFactorAuthDto, TwoFactorAuth>();
                config.CreateMap<CreateOtpDto, Otp>();
                config.CreateMap<OtpDto, Otp>().ReverseMap();
                #region AuthProto_Otp
                config.CreateMap<CreateOtpProtoDto, CreateOtpDto>().ReverseMap();
                config.CreateMap<OtpProtoDto, OtpDto>().ReverseMap();
                config.CreateMap<VerifyOTPProtoDto, VerifyOTPDto>().ReverseMap();
                #endregion
                #region AuthProto_Tfa
                config.CreateMap<TwoFactorAuthProtoDto, TwoFactorAuthDto>().ReverseMap();
                #endregion
            });

            return mappingConfig;
        }
    }
}