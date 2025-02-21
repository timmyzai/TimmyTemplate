using AutoMapper;
namespace ByteAwesome.TestAPI
{
    public static class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
            });
            return mappingConfig;
        }
    }
}