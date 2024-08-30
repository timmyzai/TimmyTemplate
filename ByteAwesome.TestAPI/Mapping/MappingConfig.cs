using AutoMapper;

namespace ByteAwesome.TestAPI
{
    public static class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<entity.Product, Models.ProductDto>();
                config.CreateMap<Models.ProductDto, entity.Product>();
                config.CreateMap<entity.Product, Models.ProductCreate>();
                config.CreateMap<Models.ProductCreate, entity.Product>();
            });
            return mappingConfig;
        }
    }
}