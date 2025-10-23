using AutoMapper;
using TestAPI.Models;
using TestAPI.Models.Dtos;
namespace TestAPI
{
    public static class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<CreateGamesDto, Games>();
                config.CreateMap<Games, GamesDto>();

                config.CreateMap<CreateGameScoresDto, GameScores>();
                config.CreateMap<GameScores, GameScoresDto>();

                config.CreateMap<Games, GameHistoryDto>();
                config.CreateMap<GamesDto, GameHistoryDto>();

                config.CreateMap<CreateScoreRuleDto, ScoreRules>();
                config.CreateMap<ScoreRules, ScoreRuleDto>().ReverseMap();
            });
            return mappingConfig;
        }
    }
}