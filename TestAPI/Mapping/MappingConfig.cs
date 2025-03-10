using AutoMapper;
using TestAPI.Dtos.BaseCurrency;
using TestAPI.Dtos.ExchangeRate;
using TestAPI.Dtos.User;
using TestAPI.Dtos.Wallet;
using TestAPI.Models;

namespace TestAPI
{
    public static class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<User, UserDto>().ReverseMap();
                config.CreateMap<CreateUserDto, User>();
                
                config.CreateMap<Wallet, WalletDto>().ReverseMap();
                config.CreateMap<CreateWalletDto, Wallet>();
                config.CreateMap<DepositIntoWalletDto, WalletDto>();
                config.CreateMap<WithdrawFromWalletDto, WalletDto>();
                
                config.CreateMap<ExchangeRate, ExchangeRateDto>();
                config.CreateMap<ExchangeRateDto, ExchangeRate>()
                    .ForMember(x => x.BaseCurrency, opt => opt.Ignore())
                    .ForMember(x => x.Id, opt => opt.Ignore());
                config.CreateMap<CreateExchangeRateDto, ExchangeRate>();
                
                config.CreateMap<BaseCurrency, BaseCurrencyDto>().ReverseMap();;
                config.CreateMap<CreateBaseCurrencyDto, BaseCurrency>();
                
                
            });
            return mappingConfig;
        }
    }
}