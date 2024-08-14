using System.ComponentModel.DataAnnotations.Schema;
using ByteAwesome.UserAPI.Models.Dtos;

namespace ByteAwesome.UserAPI.Models
{
    public class KycDocs : FullyAuditedEntity<Guid>
    {
        [ForeignKey("KycId")]
        public Guid KycId { get; set; }
        public virtual Kyc Kyc { get; set; }
        public KycDocType DocumentType { get; set; }
        public string DocumentUrl { get; set; }
    }
}