using ByteAwesome.UserAPI.Models.Dtos;
using Newtonsoft.Json;

namespace ByteAwesome.UserAPI.ExternalApis.SumsubKycService.Dto
{
    public class SumSubDocsDto : SumSubErrorResponse
    {
        [JsonProperty("IDENTITY")]
        public SumSubDocDetail Identity { get; set; }
        [JsonProperty("IDENTITY2")]
        public SumSubDocDetail Identity2 { get; set; }
        [JsonProperty("IDENTITY3")]
        public SumSubDocDetail Identity3 { get; set; }
        [JsonProperty("IDENTITY4")]
        public SumSubDocDetail Identity4 { get; set; }
        [JsonProperty("SELFIE")]
        public SumSubDocDetail Selfie { get; set; }
        [JsonProperty("SELFIE2")]
        public SumSubDocDetail Selfie2 { get; set; }
        [JsonProperty("PROOF_OF_RESIDENCE")]
        public SumSubDocDetail ProofOfResidence { get; set; }
        [JsonProperty("PROOF_OF_RESIDENCE2")]
        public SumSubDocDetail ProofOfResidence2 { get; set; }
        [JsonProperty("QUESTIONNAIRE")]
        public SumSubDocDetail Questionnaire { get; set; }
        [JsonProperty("PHONE_VERIFICATION")]
        public SumSubDocDetail PhoneVerification { get; set; }
        [JsonProperty("EMAIL_VERIFICATION")]
        public SumSubDocDetail EmailVerification { get; set; }
        [JsonProperty("PAYMENT_METHODS")]
        public SumSubDocDetail PaymentMethods { get; set; }
        [JsonProperty("E_KYC")]
        public SumSubDocDetail EKyc { get; set; }
    }

    public class SumSubDocDetail
    {
        [JsonProperty("reviewResult")]
        public SumSubReviewResult ReviewResult { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
        private string _idDocType;

        [JsonProperty("idDocType")]
        public string IdDocType
        {
            get => _idDocType;
            set => _idDocType = GeneralHelper.SetEnum(value, out DocType);
        }
        [JsonIgnore]
        public KycDocType? DocType;

        [JsonProperty("imageIds")]
        public List<long> ImageIds { get; set; }

        [JsonProperty("imageReviewResults")]
        public Dictionary<string, SumSubReviewResult> ImageReviewResults { get; set; }

        [JsonProperty("forbidden")]
        public bool? Forbidden { get; set; }

        [JsonProperty("partialCompletion")]
        public object PartialCompletion { get; set; }

        [JsonProperty("stepStatuses")]
        public object StepStatuses { get; set; }

        [JsonProperty("imageStatuses")]
        public List<object> ImageStatuses { get; set; }
    }

    public class SumSubReviewResult
    {

        private string _reviewAnswer;
        [JsonProperty("reviewAnswer")]
        public string ReviewAnswer
        {
            get => _reviewAnswer;
            set => _reviewAnswer = GeneralHelper.SetEnum(value, out Answer);
        }
        [JsonIgnore]
        public ReviewAnswer? Answer;
    }
    public enum ReviewAnswer
    {
        Green,
        Red
    }
    public class GetDocumentImageDto : SumSubErrorResponse
    {
        public byte[] Image { get; set; }
    }
}