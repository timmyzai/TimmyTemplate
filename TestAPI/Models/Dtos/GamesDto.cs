using AwesomeProject;

namespace TestAPI.Models.Dtos
{
    public class UserCreateGamesDto
    {
        public int MaxRounds { get; set; }
    }

    public class CreateGamesDto
    {
        public Guid UserId { get; set; }
        public int MaxRounds { get; set; }
        public DateTime StartTime { get; set; }
        public string FirstWord  { get; set; }
    }


    public class GamesDto : FullyAuditedEntityDto<Guid>
    {
        public Guid UserId { get; set; }
        public int FinalScore { get; set; }
        public int MaxRounds { get; set; }
        public int ActualRounds { get; set; }
        public string FirstWord { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<GameScoresDto> GameScores { get; set; }
    }

    public class GameHistoryDto
    {
        public int FinalScore { get; set; }
        public int MaxRounds { get; set; }
        public int ActualRounds { get; set; }
        public string FirstWord { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class GameLeaderboardDto
    {
        public int FinalScore { get; set; }
        public int MaxRounds { get; set; }
        public int ActualRounds { get; set; }
        public string FirstWord { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }


}