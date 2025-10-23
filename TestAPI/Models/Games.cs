using AwesomeProject;

namespace TestAPI.Models
{
  public class Games : FullyAuditedEntity<Guid>, IUserIdEntity
  {
    public Guid UserId { get; set; }
    public int FinalScore { get; set; }
    public int MaxRounds { get; set; }
    public int ActualRounds { get; set; }
    public string FirstWord { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public ICollection<GameScores> GameScores { get; internal set; }
  }
}