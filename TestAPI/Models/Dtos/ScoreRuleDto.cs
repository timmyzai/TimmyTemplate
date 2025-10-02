using AwesomeProject;

namespace TestAPI.Models.Dtos
{
    public class ScoreRuleDto : FullyAuditedEntityDto<Guid>
    {
        public int MaxSeconds { get; set; }
        public int Points { get; set; }
    }

    public class CreateScoreRuleDto
    {
        public int MaxSeconds { get; set; }
        public int Points { get; set; }
    }
}