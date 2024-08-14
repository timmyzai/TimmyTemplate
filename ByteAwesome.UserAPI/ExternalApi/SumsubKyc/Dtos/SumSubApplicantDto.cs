namespace ByteAwesome.UserAPI.ExternalApis.SumsubKycService.Dto
{
    public class SumSubApplicantDto : SumSubErrorResponse
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Key { get; set; }
        public string ClientId { get; set; }
        public string InspectionId { get; set; }
        public string ExternalUserId { get; set; }
        public SumSubApplicant_InfoDto Info { get; set; }
        public string ApplicantPlatform { get; set; }
        public string IpCountry { get; set; }
        public SumSubApplicant_AgreementDto Agreement { get; set; }
        public SumSubApplicant_RequiredIdDocsDto RequiredIdDocs { get; set; }
        public SumSubApplicant_ReviewDto Review { get; set; }
        public string Lang { get; set; }
        public string Type { get; set; }
    }

    public class SumSubApplicant_InfoDto
    {
        public string Country { get; set; }
        public List<SumSubApplicant_IdDocDto> IdDocs { get; set; }
    }

    public class SumSubApplicant_IdDocDto
    {
        public string IdDocType { get; set; }
        public string Country { get; set; }
    }

    public class SumSubApplicant_AgreementDto
    {
        public DateTime CreatedAt { get; set; }
        public DateTime AcceptedAt { get; set; }
        public string Source { get; set; }
        public List<string> Targets { get; set; }
    }

    public class SumSubApplicant_RequiredIdDocsDto
    {
        public List<SumSubApplicant_DocSetDto> DocSets { get; set; }
    }

    public class SumSubApplicant_DocSetDto
    {
        public string IdDocSetType { get; set; }
        public List<string> Types { get; set; }
        public List<string> SubTypes { get; set; }
        public string VideoRequired { get; set; }
    }

    public class SumSubApplicant_ReviewDto
    {
        public string ReviewId { get; set; }
        public string AttemptId { get; set; }
        public int AttemptCnt { get; set; }
        public string LevelName { get; set; }
        public string LevelAutoCheckMode { get; set; }
        public DateTime CreateDate { get; set; }
        public string ReviewStatus { get; set; }
        public int Priority { get; set; }
    }
    
}