using AwesomeProject;
using Microsoft.Extensions.Options;
using TestAPI.Models.Dtos;
using TestAPI.Repository;

namespace TestAPI.Services
{
    public interface IGameService
    {
        string SelectRandomWord();
    }

    public class GameService : IGameService
    {
        private readonly WordListConfig wordListConfig;
        private readonly GameRepository gameRepository;

        public GameService(
            IOptions<WordListConfig> wordListConfig,
            GameRepository gameRepository
            )
        {
            this.wordListConfig = wordListConfig.Value;
            this.gameRepository = gameRepository;
        }

        public async Task<GamesDto> CreateNewGame(UserCreateGamesDto input)
        {
            var repoInput = new CreateGamesDto
            {
                UserId = CurrentSession.GetUser().Id,
                MaxRounds = input.MaxRounds,
                StartTime = DateTime.UtcNow,
                FirstWord = SelectRandomWord()
            }; 

            return await gameRepository.Add(repoInput);

        }

        public string SelectRandomWord()
        {
            var lines = File.ReadAllLines(wordListConfig.Path);
            var r = new Random();
            var randomLineNumber = r.Next(0, lines.Length - 1);
            var line = lines[randomLineNumber];
            return line;
        }
    }
}