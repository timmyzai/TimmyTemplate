using ByteAwesome.UserAPI.Models.Dtos;

namespace ByteAwesome.UserAPI.Repository
{
    public interface IKycRepository : IBaseRepository<KycDto, KycDto, Guid>
    {
        Task<KycDto> GetByUserId(Guid userId);
    }
}