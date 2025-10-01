using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AwesomeProject;

namespace TestAPI.Models
{
    public class GameScores : FullyAuditedEntity<Guid>
    {
        [ForeignKey("GamesId")]
        public Guid GamesId { get; set; }
        public virtual Games Games { get; set; }
        public int RoundNumber { get; set; }
        public string WordPrompt { get; set; }
        public string Answer { get; set; }
        public bool IsCorrect { get; set; }
        public double Score { get; set; }
        public string Remarks { get; set; }
    }
}