using AutoMapper;
using ByteAwesome;
using ByteAwesome.SecretAPI.DbContexts;
using ByteAwesome.SecretAPI.Models;
using ByteAwesome.SecretAPI.Models.Dtos;
using ByteAwesome.SecretAPI.Repository;
using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore;

public class PasskeyRepository : BaseRepository<Passkey, PasskeyDto, CreatePasskeyDto, Guid>, IPasskeyRepository
{
    public PasskeyRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper) {}

    public async Task<IEnumerable<PasskeyDto>> GetCredentialsByUser(string username)
    {
        var creds = await Query().Where(p => p.UserName == username).ToListAsync();
        return MapEntitiesToDtos(creds);
    }

    public async Task<List<PublicKeyCredentialDescriptor>> GetCredentialDescriptorsByUser(string username)
    {
        return await Query()
            .Where(p => p.UserName == username)
            .Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))
            .ToListAsync();
    }
    
    public async Task<PasskeyDto> GetCredentialById(byte[] credentialId)
    {
        var cred = await Query().FirstOrDefaultAsync(p => p.CredentialId == credentialId);
        return MapEntityToDto(cred);
    }

    public async Task<IEnumerable<PasskeyDto>> GetCredentialsByUserHandle(byte[] userHandle)
    {
        var creds = await Query().Where(p => p.UserHandle == userHandle).ToListAsync();
        return MapEntitiesToDtos(creds);
    }
}
