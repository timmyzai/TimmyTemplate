using ByteAwesome.SecretAPI.Models.Dtos;

namespace ByteAwesome.SecretAPI.Repository
{
    public interface IOtpRepository : IBaseRepository<OtpDto, CreateOtpDto, Guid>
    {
        Task<OtpDto> GetValidOtpByValueAndType(string value, OtpActionType ActionType);
    }
}
