using ByteAwesome.SecretAPI.Models.Dtos;
using Fido2NetLib.Objects;

namespace ByteAwesome.SecretAPI.Repository
{
    public interface IPasskeyRepository : IBaseRepository<PasskeyDto, CreatePasskeyDto, Guid>
    {
        Task<PasskeyDto> GetCredentialById(byte[] credentialId);
        Task<List<PublicKeyCredentialDescriptor>> GetCredentialDescriptorsByUser(string username);
        Task<IEnumerable<PasskeyDto>> GetCredentialsByUser(string username);
        Task<IEnumerable<PasskeyDto>> GetCredentialsByUserHandle(byte[] userHandle);
    }
}