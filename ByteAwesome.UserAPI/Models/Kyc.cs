using System.ComponentModel.DataAnnotations.Schema;
using ByteAwesome.UserAPI.Models.Dtos;

namespace ByteAwesome.UserAPI.Models
{
    public class Kyc : FullyAuditedEntity<Guid>
    {
        [ForeignKey("UserId")]
        public Guid UserId { get; set; }
        public virtual Users User { get; set; }
        public KycStatus Status { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public ICollection<KycDocs> KycDocs { get; set; }
    }
}