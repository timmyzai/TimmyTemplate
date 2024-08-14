

namespace ByteAwesome.UserAPI.Models.Dtos
{
    public class KycDocsDto : FullyAuditedEntityDto<Guid>
    {
        public KycDocType DocumentType { get; set; } = KycDocType.Unknown;
        public string DocumentUrl { get; set; }
    }
    public enum KycDocType
    {
        ID_CARD = 1,
        PASSPORT = 2,
        DRIVERS = 3,
        SELFIE = 4,
        Unknown = 999
    }
}