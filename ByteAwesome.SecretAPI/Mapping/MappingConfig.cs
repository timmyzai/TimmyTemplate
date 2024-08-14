using AutoMapper;
using ByteAwesome.SecretAPI.Grpc;
using ByteAwesome.SecretAPI.Models;
using ByteAwesome.SecretAPI.Models.Dtos;

namespace ByteAwesome.SecretAPI
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
                #region AuthProto_AccessToken
                config.CreateMap<GenerateAccessTokenResult, GenerateAccessTokenProtoResult>().ReverseMap();
                #endregion
                #region KmsKeys
                config.CreateMap<KmsKeysDto, KmsKeys>().ReverseMap();
                #endregion
                #region Passkey
                config.CreateMap<PasskeyDto, Passkey>().ReverseMap();
                config.CreateMap<CreatePasskeyDto, Passkey>().ReverseMap();
                #endregion
            });

            return mappingConfig;
        }
    }
}