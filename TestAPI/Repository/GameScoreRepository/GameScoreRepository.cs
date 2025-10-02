using AutoMapper;
using AwesomeProject;
using Microsoft.EntityFrameworkCore;
using TestAPI.DbContexts;
using TestAPI.Models;
using TestAPI.Models.Dtos;

namespace TestAPI.Repository
{
    public interface IGameScoreRepository
    {
        Task<GameScoresDto> GetById(Guid id, bool isBoRequest = false);
    }

    public class GameScoreRepository : BaseRepository<GameScores, GameScoresDto, CreateGameScoresDto, Guid>, IGameScoreRepository
    {

        public GameScoreRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<bool> CheckIfWordIsUsed(Guid GameId, string word)
        {
            return await Query()
                .Where(g => g.GamesId == GameId)
                .AnyAsync(s => s.Answer.ToLower() == word.ToLower());
        }

    }
}