using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeProject;

namespace TestAPI.Models.Dtos
{
    public class CreateGameScoresDto
    {
        public Guid GamesId { get; set; }
        public int RoundNumber { get; set; }
        public string WordPrompt { get; set; }
        public string Answer { get; set; }
        public bool IsCorrect { get; set; }
        public double Score { get; set; }
        public DateTime AnswerStartTime { get; set; }
        public string Remarks { get; set; }
    }

    public class GameScoresDto : FullyAuditedEntityDto<Guid>
    {
        public Guid GamesId { get; set; }
        public int RoundNumber { get; set; }
        public string WordPrompt { get; set; }
        public string Answer { get; set; }
        public bool IsCorrect { get; set; }
        public double Score { get; set; }
        public DateTime AnswerStartTime { get; set; }
        public string Remarks { get; set; }
    }

    public class PrelimGameScoreDetails
    {
        public Guid GamesId { get; set; }
        public int RoundNumber { get; set; }
        public string WordPrompt { get; set; }
        public DateTime AnswerStartTime { get; set; }
        public int MaxRounds { get; set; }
    }


    public class SubmitWordDto
    {
        public Guid GameId { get; set; }
        public string Word { get; set; }
    }

}