using Newtonsoft.Json;

namespace ByteAwesome.UserAPI.ExternalApis.SumsubKycService.Dto
{
    public class SumSubAccessTokenDto : SumSubErrorResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("userId")]
        public Guid UserId { get; set; }
    }
}