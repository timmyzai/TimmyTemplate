using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeProject;

namespace TestAPI.Models
{
    public class ScoreRules : FullyAuditedEntity<Guid>
    {
        public int MaxSeconds { get; set; }
        public int Points { get; set; }
    }
}