using AutoMapper;
using ByteAwesome.Services;
using ByteAwesome.UserAPI.Models;
using ByteAwesome.UserAPI.Models.Dtos;

namespace ByteAwesome.UserAPI
{
    public static class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                #region Users
                config.CreateMap<UserDto, Users>()
                    .ForMember(dest => dest.Roles, opt => opt.Ignore());
                config.CreateMap<Users, UserDto>()
                    .ForMember(dest => dest.UserData, opt => opt.MapFrom(src => src))
                    .ReverseMap();
                config.CreateMap<CreateUserDto, Users>();
                config.CreateMap<Users, Bo_UserDataDto>()
                    .ForMember(dest => dest.RoleNames, opt => opt.Ignore())
                    .ReverseMap();
                config.CreateMap<UserDto, EntityUserDto>().ReverseMap();
                config.CreateMap<UserDto, UserProfileDto>()
                    .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.UserData.EmailAddress))
                    .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.UserData.FirstName))
                    .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.UserData.LastName))
                    .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.UserData.PhoneNumber))
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserData.UserName))
                    .ReverseMap();
                config.CreateMap<UserDto, UserBasicInfoDto>()
                    .ForMember(dest => dest.UserBasicInfoData, opt => opt.MapFrom(src => src.UserData))
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ReverseMap();
                config.CreateMap<UserBasicInfoDataDto, Bo_UserDataDto>().ReverseMap();
                config.CreateMap<UserDto, UserBasicInfoDataDto>()
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserData.UserName))
                    .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.UserData.PhoneNumber))
                    .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.UserData.EmailAddress))
                    .ReverseMap();
                #endregion
                #region UserRoles
                config.CreateMap<RolesDto, Roles>().ReverseMap();
                config.CreateMap<CreateRolesDto, Roles>();
                config.CreateMap<CreateUserRolesDto, UserRoles>();
                config.CreateMap<UserRolesDto, UserRoles>().ReverseMap();
                #endregion
                #region KYC
                config.CreateMap<KycDto, Kyc>().ReverseMap();
                config.CreateMap<KycDocsDto, KycDocs>().ReverseMap();
                #endregion
                #region UserLoginSessionInfoDto
                config.CreateMap<UserLoginSessionInfoDto, UserLoginSessionInfo>().ReverseMap();
                config.CreateMap<DeviceInfo, UserLoginSessionInfoDto>();
                config.CreateMap<LocationInfo, UserLoginSessionInfoDto>();
                #endregion

            });

            return mappingConfig;
        }
    }
}