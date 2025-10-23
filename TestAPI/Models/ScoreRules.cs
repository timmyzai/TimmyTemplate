using AwesomeProject;

namespace TestAPI.Models
{
    public class ScoreRules : FullyAuditedEntity<Guid>
    {
        public int MaxSeconds { get; set; }
        public int Points { get; set; }
    }
}