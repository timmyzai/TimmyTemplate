

namespace ByteAwesome.UserAPI.Models.Dtos
{
    public class KycDto : FullyAuditedEntityDto<Guid>
    {
        public KycStatus Status { get; set; } = KycStatus.NotStarted;
        public DateTime? ApprovedDate { get; set; }
        public List<KycDocsDto> KycDocs { get; set; } = new List<KycDocsDto>();
    }
    public enum KycStatus
    {
        NotStarted = 0,
        PendingUploadDocument = 1,
        PendingReview = 2,
        Approved = 3,
        RetryRejected = 4,
        FinalRejected = 5,
        OnHold = 6
    }
}