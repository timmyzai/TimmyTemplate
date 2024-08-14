using AutoMapper;
using ByteAwesome.SecretAPI.DbContexts;
using ByteAwesome.SecretAPI.Models;
using ByteAwesome.SecretAPI.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ByteAwesome.SecretAPI.Repository
{
    public class KmsKeysRepository : BaseRepository<KmsKeys, KmsKeysDto, KmsKeysDto, Guid>, IKmsKeysRepository
    {
        public KmsKeysRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        { }
        public async Task<KmsKeysDto> GetByUserId(Guid userId)
        {
            var item = await Query().FirstOrDefaultAsync(x => x.UserId == userId);
            if (item is null)
            {
                return default;
            }
            return MapEntityToDto(item);
        }
    }
}
