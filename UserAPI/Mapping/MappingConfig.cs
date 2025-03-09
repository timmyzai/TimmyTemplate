using AutoMapper;
using UserAPI.Models;
using UserAPI.Models.Dtos;

namespace UserAPI
{
    public static class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                #region Users
                config.CreateMap<UserDto, Users>().IncludeMembers(src => src.UserData).ReverseMap();
                config.CreateMap<Bo_UserDataDto, Users>().ForMember(dest => dest.Roles, opt => opt.Ignore());
                config.CreateMap<Users, Bo_UserDataDto>().ForMember(dest => dest.RoleNames, opt => opt.Ignore());
                config.CreateMap<CreateUserDto, Users>();
                config.CreateMap<UserDto, EntityUserDto>().ReverseMap();
                config.CreateMap<UserDto, UserProfileDto>()
                    .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.UserData.EmailAddress))
                    .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.UserData.FirstName))
                    .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.UserData.LastName))
                    .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.UserData.PhoneNumber))
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserData.UserName))
                    .ReverseMap();
                #endregion
                #region UserRoles
                config.CreateMap<RolesDto, Roles>().ReverseMap();
                config.CreateMap<CreateRolesDto, Roles>();
                config.CreateMap<CreateUserRolesDto, UserRoles>();
                config.CreateMap<UserRolesDto, UserRoles>().ReverseMap();
                #endregion
                #region RefreshToken
                config.CreateMap<RefreshTokenDto, RefreshToken>().ReverseMap();
                #endregion
            });

            return mappingConfig;
        }
    }
}