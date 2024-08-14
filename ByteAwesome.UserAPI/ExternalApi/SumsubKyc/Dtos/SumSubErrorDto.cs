using System.Text.Json.Serialization;

namespace ByteAwesome.UserAPI.ExternalApis.SumsubKycService.Dto
{
    public class SumSubErrorResponse
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; }

        [JsonPropertyName("errorCode")]
        public int? ErrorCode { get; set; }

        [JsonPropertyName("errorName")]
        public string ErrorName { get; set; }
    }
}