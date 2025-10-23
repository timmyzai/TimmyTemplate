using TestAPI.Models.Dtos;
using TestAPI.Models.Word;
using TestAPI.Repository;
using TestAPI.Repository.ScoreRuleRepository;

namespace TestAPI.Services
{
    public interface IGameScoreServices
    {
        Task<GamesDto> SubmitWord(Guid GameId, string word);
    }

    public class GameScoreServices : IGameScoreServices
    {

        private readonly DictionaryApiClient dictionaryApi;
        private readonly GameScoreRepository gameScoreRepository;
        private readonly GameRepository gameRepository;
        private readonly ScoreRuleRepository scoreRuleRepository;

        public GameScoreServices(
            DictionaryApiClient dictionaryApi,
            GameScoreRepository gameScoreRepository,
            GameRepository gameRepository,
            ScoreRuleRepository scoreRuleRepository
            )
        {
            this.dictionaryApi = dictionaryApi;
            this.gameScoreRepository = gameScoreRepository;
            this.gameRepository = gameRepository;
            this.scoreRuleRepository = scoreRuleRepository;
        }

        private record WordCheckResult(
            bool IsValid,
            string Remarks,
            int Score = 0,
            List<WordDefinition> Definitions = null
        );

        private async Task<WordCheckResult> CheckWord(Guid gameId, string word, string prompt, DateTime answerStartTime)
        {
            if (!IsStartWithCorrectLetter(prompt, word))
            {
                return new(false, $"Your word must begin with '{prompt[^1]}'");
            }

            if (await gameScoreRepository.CheckIfWordIsUsed(gameId, word))
            {
                return new(false, "You used this word before. Game over.");
            }

            var definitions = await GetWordDefinition(word);
            if (definitions.Count <= 0)
            {
                return new(false, "Not a real word. Game over.", Definitions: definitions);
            }

            var score = await CalculateScore(word, answerStartTime);
            return new(true, $"Correct! +{score} points awarded!", score, definitions);
        }


        public async Task<GamesDto> SubmitWord(Guid GameId, string word)
        {
            var prelimDetails = await GetNewGameScoreDetailsByGameId(GameId);

            var result = await CheckWord(GameId, word, prelimDetails.WordPrompt, prelimDetails.AnswerStartTime);

            await gameScoreRepository.Add(new CreateGameScoresDto
            {
                GamesId = GameId,
                RoundNumber = prelimDetails.RoundNumber,
                WordPrompt = prelimDetails.WordPrompt,
                Answer = word,
                IsCorrect = result.IsValid,
                Score = result.Score,
                Remarks = result.Remarks
            });

            // check if game ended
            if (!result.IsValid || prelimDetails.RoundNumber == prelimDetails.MaxRounds)
            {
                await FinalizeGame(GameId);
            }

            var game = await gameRepository.GetById(GameId);

            return game;
        }

        private async Task<GamesDto> FinalizeGame(Guid id)
        {
            var game = await gameRepository.GetById(id);

            var totalScore = game.GameScores.Sum(x => x.Score);
            var gameRounds = game.GameScores.Max(x => x.RoundNumber);

            game.FinalScore = totalScore;
            game.ActualRounds = gameRounds;
            game.EndTime = DateTime.UtcNow;

            var finalizedGame = await gameRepository.PatchGameFinal(game);

            return finalizedGame;
        }

        private async Task<PrelimGameScoreDetails> GetNewGameScoreDetailsByGameId(Guid GameId)
        {

            var game = await gameRepository.GetById(GameId);

            var lastScore = game.GameScores
                            .OrderByDescending(s => s.RoundNumber)
                            .FirstOrDefault();

            var nextRound = (lastScore?.RoundNumber ?? 0) + 1;
            var answerStartTime = lastScore?.CreatedTime ?? game.StartTime;
            var wordPrompt = lastScore?.Answer ?? game.FirstWord;

            return new PrelimGameScoreDetails
            {
                GamesId = GameId,
                RoundNumber = nextRound,
                AnswerStartTime = answerStartTime,
                WordPrompt = wordPrompt,
                MaxRounds = game.MaxRounds
            };

        }

        private async Task<List<WordDefinition>> GetWordDefinition(string word)
        {
            var result = new List<WordDefinition>();
            try
            {
                result = await dictionaryApi.GetWordDefinitionAsync(word);
            }
            catch (Exception)
            {
            }
            return result;
        }

        private bool IsStartWithCorrectLetter(string wordPrompt, string answer)
        {
            return wordPrompt.EndsWith(answer[0]);
        }

        private async Task<int> CalculateScore(string answer, DateTime answerStartTime)
        {
            int lengthScore = answer.Length;

            DateTime endTime = DateTime.UtcNow;

            var durationInSecs = endTime.Subtract(answerStartTime);

            var scoreRules = await scoreRuleRepository.Get();

            var rule = scoreRules
                        .OrderBy(r => r.MaxSeconds)
                        .FirstOrDefault(r => durationInSecs.TotalSeconds <= r.MaxSeconds);

            int timeScore = rule?.Points ?? 0;

            return lengthScore + timeScore;

        }



    }
}