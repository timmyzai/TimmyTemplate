using System.Security.Cryptography;
using System.Text;
using ByteAwesome.UserAPI.ExternalApis.SumsubKyc.Modules;
using ByteAwesome.UserAPI.ExternalApis.SumsubKycService.Dto;
using ByteAwesome.UserAPI.Models.Dtos;
using ByteAwesome.UserAPI.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ByteAwesome.UserAPI.ExternalApis.SumsubKycService
{
    public interface ISumsubKycService
    {
        Task<SumSubAccessTokenDto> GetAccessTokenAsync(string userId, string levelName);
        Task<SumSubApplicantDto> GetApplicantDataAsync(string applicantId);
        Task<SumSubApplicantDto> GetApplicantDataByExternalUserIdAsync(string externalUserId);
        Task<SumSubDocsDto> GetApplicantDocumentsStatusAsync(string applicantId);
        Task<KycDocsDto> GetAndSaveDocumentImageAsync(string inspectionId, long imageId, string userName, string docType);
    }
    [AllowAnonymous]
    public class SumsubKycService : BaseController, ISumsubKycService
    {
        private readonly IOptions<SumsubKycModuleConfig> sumsubKycConfig;
        private readonly HttpClient _httpClient;
        private readonly IOptions<AppModuleConfig> appConfig;
        public SumsubKycService(
            IOptions<SumsubKycModuleConfig> sumsubKycConfig,
            IHttpClientFactory httpClientFactory,
            IOptions<AppModuleConfig> appConfig
        )
        {
            this.sumsubKycConfig = sumsubKycConfig;
            this.appConfig = appConfig;
            _httpClient = httpClientFactory.CreateClient(nameof(SumsubKycService));
        }
        public async Task<SumSubApplicantDto> GetApplicantDataAsync(string applicantId)
        {
            if (string.IsNullOrEmpty(applicantId))
            {
                throw new ArgumentException("Applicant ID cannot be null or empty.", nameof(applicantId));
            }
            var urlPath = $"/resources/applicants/{applicantId}/one";
            var httpRequestMsg = HttpClientHelper.CreateRequestMessage(HttpMethod.Get, urlPath);
            HttpClientHelper.AddHeader(httpRequestMsg, SumsubTokenHeader(urlPath, httpRequestMsg));
            var result = await HttpClientHelper.SendRequestAsync<SumSubApplicantDto>(_httpClient, httpRequestMsg);
            ErrorHandling(result);
            return result;
        }
        public async Task<SumSubApplicantDto> GetApplicantDataByExternalUserIdAsync(string externalUserId)
        {
            if (string.IsNullOrEmpty(externalUserId))
            {
                throw new ArgumentException("External User ID cannot be null or empty.", nameof(externalUserId));
            }
            var urlPath = $"/resources/applicants/-;externalUserId={externalUserId}/one";
            var httpRequestMsg = HttpClientHelper.CreateRequestMessage(HttpMethod.Get, urlPath);
            HttpClientHelper.AddHeader(httpRequestMsg, SumsubTokenHeader(urlPath, httpRequestMsg));
            var result = await HttpClientHelper.SendRequestAsync<SumSubApplicantDto>(_httpClient, httpRequestMsg);
            ErrorHandling(result);
            return result;
        }
        public async Task<SumSubDocsDto> GetApplicantDocumentsStatusAsync(string applicantId)
        {
            var urlPath = $"/resources/applicants/{applicantId}/requiredIdDocsStatus";
            var httpRequestMsg = HttpClientHelper.CreateRequestMessage(HttpMethod.Get, urlPath);
            HttpClientHelper.AddHeader(httpRequestMsg, SumsubTokenHeader(urlPath, httpRequestMsg));
            var result = await HttpClientHelper.SendRequestAsync<SumSubDocsDto>(_httpClient, httpRequestMsg);
            ErrorHandling(result);
            return result;
        }
        public async Task<KycDocsDto> GetAndSaveDocumentImageAsync(string inspectionId, long imageId, string userName, string docType)
        {
            var urlPath = $"/resources/inspections/{inspectionId}/resources/{imageId}";
            var httpRequestMsg = HttpClientHelper.CreateRequestMessage(HttpMethod.Get, urlPath);
            HttpClientHelper.AddHeader(httpRequestMsg, SumsubTokenHeader(urlPath, httpRequestMsg));
            var imageResponse = await HttpClientHelper.SendRequestAsync<GetDocumentImageDto>(_httpClient, httpRequestMsg);
            ErrorHandling(imageResponse);
            var uploadFolder = Path.Combine(appConfig.Value.UploadRootFolder, "kyc", userName);
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var fileName = $"{docType}_{timestamp}.png";
            var filePath = Path.Combine(uploadFolder, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, imageResponse.Image);
            var uploadFolderdb = Path.Combine(appConfig.Value.UploadDB, "kyc", userName);
            var fileURL = Path.Combine(uploadFolderdb, fileName);
            _ = GeneralHelper.SetEnum(docType, out KycDocType? DocType);
            var result = new KycDocsDto
            {
                DocumentType = DocType ?? KycDocType.Unknown,
                DocumentUrl = fileURL
            };
            return result;
        }
        public async Task<SumSubAccessTokenDto> GetAccessTokenAsync(string userId, string levelName)
        {
            var urlPath = $"/resources/accessTokens?userId={userId}&levelName={levelName}";
            var httpRequestMsg = HttpClientHelper.CreateRequestMessage(HttpMethod.Post, urlPath, "");
            HttpClientHelper.AddHeader(httpRequestMsg, SumsubTokenHeader(urlPath, httpRequestMsg));
            var result = await HttpClientHelper.SendRequestAsync<SumSubAccessTokenDto>(_httpClient, httpRequestMsg);
            ErrorHandling(result);
            return result;
        }
        #region Private Methods
        private Dictionary<string, string> SumsubTokenHeader(string urlPath, HttpRequestMessage body = null)
        {
            long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return new Dictionary<string, string> {
                { "X-App-Token", sumsubKycConfig.Value.AppToken },
                { "X-App-Access-Sig", CreateSignature(ts, body.Method, urlPath, body) },
                { "X-App-Access-Ts", ts.ToString() }
            };
        }
        private string CreateSignature(long ts, HttpMethod httpMethod, string path, HttpRequestMessage body)
        {
            var hmac256 = new HMACSHA256(Encoding.ASCII.GetBytes(sumsubKycConfig.Value.SecretKey));
            byte[] byteArray = Encoding.ASCII.GetBytes(ts + httpMethod.Method + path);
            if (body is not null)
            {
                var s = new MemoryStream();
                s.Write(byteArray, 0, byteArray.Length);
                var bodyBytes = body.Content is null ? new byte[] { } : body.Content.ReadAsByteArrayAsync().Result;
                s.Write(bodyBytes, 0, bodyBytes.Length);
                byteArray = s.ToArray();
            }
            var result = hmac256.ComputeHash(new MemoryStream(byteArray)).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
            return result;
        }
        private void ErrorHandling(SumSubErrorResponse errResponse)
        {
            if (string.IsNullOrEmpty(errResponse.Description)) return;
            if (errResponse is null)
            {
                throw new Exception("Sumsub response is null");
            }
            var errorMessage = $"An error occurred: {errResponse.Description}, Code: {errResponse.Code}, Correlation ID: {errResponse.CorrelationId}";
            if (errResponse.ErrorCode is not null)
            {
                errorMessage += $", Error Code: {errResponse.ErrorCode}";
            }
            if (!string.IsNullOrEmpty(errResponse.ErrorName))
            {
                errorMessage += $", Error Name: {errResponse.ErrorName}";
            }
            throw new Exception(errorMessage);
        }
        #endregion
    }
}
