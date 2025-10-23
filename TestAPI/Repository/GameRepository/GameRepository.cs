using AutoMapper;
using AwesomeProject;
using Microsoft.EntityFrameworkCore;
using TestAPI.DbContexts;
using TestAPI.Models;
using TestAPI.Models.Dtos;

namespace TestAPI.Repository
{
    public class GameRepository : BaseRepository<Games, GamesDto, CreateGamesDto, Guid>
    {
        public GameRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {
        }


        public override async Task<GamesDto> GetById(Guid id, bool isBoRequest = false)
        {
            var item = await Query(isBoRequest: isBoRequest)
            .Include(g => g.GameScores)
            .FirstOrDefaultAsync(r => Equals(r.Id, id));
            if (item is null)
            {
                return default;
            }
            return MapEntityToDto(item);
        }

        public async Task<GamesDto> PatchGameFinal(GamesDto input)
        {
            var item = await ContextEntity().FindAsync(input.Id) ?? throw new Exception(ErrorCodes.General.EntityNameNotFound);
            item.FinalScore = input.FinalScore;
            item.ActualRounds = input.ActualRounds;
            item.EndTime = input.EndTime;

            await context.SaveChangesAsync();

            return mapper.Map<GamesDto>(item);
        }

        public override async Task<IEnumerable<GamesDto>> Get(PaginationRequestDto pagingFilter = null, bool isBoRequest = false)
        {
            var query = Query(isBoRequest);
            query = CursorPagination(query, pagingFilter);
            var items = await query
                .Where(g => g.EndTime != null) // only show completed games
                .ToListAsync();
            return MapEntitiesToDtos(items);
        }

        public async Task<IEnumerable<GamesDto>> GetByScoreDesc(PaginationRequestDto pagingFilter = null, bool isBoRequest = false)
        {
            var query = Query(isBoRequest);
            query = CursorPagination(query, pagingFilter);
            var items = await query
                .Where(g => g.EndTime != null) // only show completed games
                .OrderByDescending(g => g.FinalScore)
                .ToListAsync();
            return MapEntitiesToDtos(items);
        }

    }
}