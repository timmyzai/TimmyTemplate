using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ByteAwesome.UserAPI.Models.Dtos;
using ByteAwesome.UserAPI.Models;
using ByteAwesome.UserAPI.DbContexts;

namespace ByteAwesome.UserAPI.Repository
{
    public class KycRepository : BaseRepository<Kyc, KycDto, KycDto, Guid>, IKycRepository
    {
        public KycRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper) { }
        public async Task<KycDto> GetByUserId(Guid userId)
        {
            var item = await Query().FirstOrDefaultAsync(r => r.UserId == userId);
            if (item is null)
            {
                return default;
            }
            return MapEntityToDto(item);
        }
        protected override IQueryable<Kyc> Query(bool isBoRequest = false)
        {
            return base.Query( isBoRequest).Include(x => x.KycDocs);
        }
    }
}