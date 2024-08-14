using ByteAwesome.SecretAPI.Models.Dtos;

namespace ByteAwesome.SecretAPI.Repository
{
    public interface IKmsKeysRepository : IBaseRepository<KmsKeysDto, KmsKeysDto, Guid>
    {
        Task<KmsKeysDto> GetByUserId(Guid userId);
    }
}
