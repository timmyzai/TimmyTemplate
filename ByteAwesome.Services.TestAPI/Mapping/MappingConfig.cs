using AutoMapper;
using ByteAwesome.Services.TestAPI.Models;
using ByteAwesome.Services.TestAPI.Models.Dtos;

namespace ByteAwesome.Services.TestAPI
{
    public static class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<ProductDto, Product>().ReverseMap();
                config.CreateMap<CreateProductDto, Product>();
            });

            return mappingConfig;
        }
    }
}