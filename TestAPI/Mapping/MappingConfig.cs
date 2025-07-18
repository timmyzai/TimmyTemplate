using AutoMapper;
using TestAPI.Models;
using TestAPI.Models.Dtos;
namespace TestAPI
{
    public static class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {

                config.CreateMap<CreateCryptoOrderDto, CryptoOrder>();
                config.CreateMap<CryptoOrder, CryptoOrderDto>().ReverseMap();
                config.CreateMap<UpdateCryptoOrderDto, CryptoOrder>();
            });
            return mappingConfig;
        }
    }
}