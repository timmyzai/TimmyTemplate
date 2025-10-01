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
                // config.CreateMap<GamesDto, Games>().IncludeMembers(src => src.UserData).ReverseMap();
                config.CreateMap<CreateGamesDto, Games>().ReverseMap();
                config.CreateMap<CreateGamesDto, GamesDto>().ReverseMap();
                config.CreateMap<Games, GamesDto>().ReverseMap();

                config.CreateMap<GameScores, GameScoresDto>().ReverseMap();
                config.CreateMap<GameScores, CreateGameScoresDto>().ReverseMap();
                config.CreateMap<GameScoresDto, CreateGameScoresDto>().ReverseMap();

                config.CreateMap<GameHistoryDto, GamesDto>().ReverseMap();
                config.CreateMap<GameHistoryDto, Games>().ReverseMap();

                config.CreateMap<ScoreRules, ScoreRuleDto>().ReverseMap();
                config.CreateMap<ScoreRules, CreateScoreRuleDto>().ReverseMap();
                config.CreateMap<ScoreRuleDto, CreateScoreRuleDto>().ReverseMap();
            });
            return mappingConfig;
        }
    }
}