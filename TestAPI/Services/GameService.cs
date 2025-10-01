using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeProject;
using Microsoft.AspNetCore.Authorization;
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

        public async Task<GamesDto> CreateNewGame(CreateGamesDto input)
        {
            var currentUserId = CurrentSession.GetUser().Id;
            input.UserId = currentUserId;
            input.StartTime = DateTime.Now;
            input.FirstWord = SelectRandomWord();

            var word = await gameRepository.Add(input);

            return word;
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