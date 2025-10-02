using AutoMapper;
using AwesomeProject;
using TestAPI.DbContexts;
using TestAPI.Models;
using TestAPI.Models.Dtos;

namespace TestAPI.Repository.ScoreRuleRepository
{
    public class ScoreRuleRepository : BaseRepository<ScoreRules, ScoreRuleDto, CreateScoreRuleDto, Guid>
    {
        public ScoreRuleRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}