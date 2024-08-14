using ByteAwesome.SecretAPI.Models.Dtos;

namespace ByteAwesome.SecretAPI.Repository
{
    public interface ITwoFactorAuthRepository : IBaseRepository<TwoFactorAuthDto, CreateTwoFactorAuthDto, Guid>
    {
        Task<TwoFactorAuthDto> GetByUserId(Guid userId);
    }
}