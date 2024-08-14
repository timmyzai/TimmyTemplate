using System.Reflection;
using System.Text.Json;
using ByteAwesome.UserAPI.ExternalApis.SumsubKycService;
using ByteAwesome.UserAPI.ExternalApis.SumsubKycService.Dto;
using ByteAwesome.UserAPI.Models.Dtos;
using ByteAwesome.UserAPI.Models.WebHook.Dtos;
using ByteAwesome.UserAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;

namespace ByteAwesome.UserAPI.Controllers
{
    public class SumsubWebHooksController : WebHook_BaseController
    {
        private readonly ISumsubKycService sumsubKycService;
        private readonly IUserRepository userRepository;
        private readonly IKycRepository kycRepository;

        public SumsubWebHooksController(
            ISumsubKycService sumsubKycService,
            IUserRepository userRepository,
            IKycRepository kycRepository
        )
        {
            this.sumsubKycService = sumsubKycService;
            this.userRepository = userRepository;
            this.kycRepository = kycRepository;
        }
        public async Task<IActionResult> SumsubWebHookEvent([FromBody] JsonElement webHookEventJson)
        {
            try
            {
                if (webHookEventJson.ValueKind != JsonValueKind.Object)
                {
                    throw new Exception("Invalid WebHook Event.");
                }
                Log.Information($"SumsubWebHookEvent - Start at {DateTime.UtcNow}");
                var webHookRawText = webHookEventJson.GetRawText();
                Log.Information($"SumsubWebHookEvent JSON: {webHookRawText}");
                var webHookEvent = JsonConvert.DeserializeObject<SumsubWebHookEventDto>(webHookRawText);
                await HandleWebHookEvent(webHookEvent);
                return Ok(new { Message = "Successful Handled" });
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                return Ok(new { Message = "Handled with Error." });
            }
        }
        public async Task HandleApplicantCreatedEvent(SumsubWebHookEventDto? webHookEvent = null)
        {
            Log.Information($"Handling ApplicantPending event for ApplicantID: {webHookEvent.ApplicantId}");
            var user = await GetUser(webHookEvent.ExternalUserId);
            if (user.UserData.KycData.Status != KycStatus.NotStarted)
            {
                throw new Exception($"Can not update to PendingUploadDocument. User {user.UserData.UserName} is not in NotStarted status.");
            }
            await GetApplicant(webHookEvent.ApplicantId, webHookEvent.ExternalUserId);
            user.UserData.KycData.Status = KycStatus.PendingUploadDocument;
            await userRepository.Update(user);
        }
        public async Task<SumSubApplicantDto> GetApplicant(string applicantId, string userId)
        {
            SumSubApplicantDto result;
            try
            {
                result = await sumsubKycService.GetApplicantDataAsync(applicantId);
            }
            catch
            {
                result = await sumsubKycService.GetApplicantDataByExternalUserIdAsync(userId);
            }
            if (result is null)
            {
                throw new KeyNotFoundException("Applicant not found in Sumsub.");
            }
            return result;
        }
        #region Private Methods
        private async Task HandleWebHookEvent(SumsubWebHookEventDto webHookEvent)
        {
            switch (webHookEvent.EventType.Value)
            {
                case SumsubEventType.ApplicantCreated:
                    await HandleApplicantCreatedEvent(webHookEvent);
                    break;
                case SumsubEventType.ApplicantPending:
                    await HandleApplicantPendingEvent(webHookEvent);
                    break;
                case SumsubEventType.ApplicantActionOnHold:
                    await HandleApplicantPendingEvent(webHookEvent);
                    break;
                case SumsubEventType.ApplicantReviewed:
                    await HandleApplicantReviewedEvent(webHookEvent);
                    break;
                default:
                    Log.Information($"Unhandled event type: {webHookEvent.EventType.Value}");
                    break;
            }
        }
        private async Task HandleApplicantPendingEvent(SumsubWebHookEventDto webHookEvent)
        {
            Log.Information($"Handling ApplicantPending event for ApplicantID: {webHookEvent.ApplicantId}");
            var user = await GetUser(webHookEvent.ExternalUserId);
            if (user.UserData.KycData.Status != KycStatus.PendingUploadDocument)
            {
                throw new Exception($"Can not update to PendingReview. User {user.UserData.UserName} is not in PendingUploadDocument status.");
            }
            user.UserData.KycData.Status = KycStatus.PendingReview;
            await userRepository.Update(user);
            SumSubDocsDto docs = await sumsubKycService.GetApplicantDocumentsStatusAsync(webHookEvent.ApplicantId);
            Dictionary<string, List<long>> imageIds = ProcessImagesTypesAndIds(docs);
            List<KycDocsDto> kycDocs = new List<KycDocsDto>();
            foreach (var docType in imageIds)
            {
                foreach (var imageId in docType.Value)
                {
                    var doc = await sumsubKycService.GetAndSaveDocumentImageAsync(webHookEvent.InspectionId, imageId, user.UserData.UserName, docType.Key);
                    kycDocs.Add(doc);
                }
            }
            var kyc = await kycRepository.GetById(user.UserData.KycData.Id);
            kyc.KycDocs = kycDocs;
            await kycRepository.Update(kyc);
        }
        private async Task HandleApplicantOnHoldEvent(SumsubWebHookEventDto webHookEvent)
        {
            Log.Information($"Handling ApplicantOnHold event for ApplicantID: {webHookEvent.ApplicantId}");
            var user = await GetUser(webHookEvent.ExternalUserId);
            if (user.UserData.KycData.Status == KycStatus.FinalRejected || user.UserData.KycData.Status == KycStatus.Approved)
            {
                throw new Exception($"Can not update to OnHold. User {user.UserData.UserName} is already in FinalRejected or Approved status.");
            }
            user.UserData.KycData.Status = KycStatus.OnHold;
            await userRepository.Update(user);
        }
        private async Task HandleApplicantReviewedEvent(SumsubWebHookEventDto webHookEvent)
        {
            Log.Information($"Handling ApplicantOnHold event for ApplicantID: {webHookEvent.ApplicantId}");
            var user = await GetUser(webHookEvent.ExternalUserId);
            if (user.UserData.KycData.Status != KycStatus.PendingReview && user.UserData.KycData.Status != KycStatus.OnHold)
            {
                throw new Exception($"Can not update to Approved or Reject. User {user.UserData.UserName} is not in PendingReview or Onhold status.");
            }
            switch (webHookEvent.ReviewResult.Answer)
            {
                case ReviewAnswer.Green:
                    Log.Information($"Applicant {webHookEvent.ApplicantId} has been approved.");
                    user.UserData.KycData.Status = KycStatus.Approved;
                    user.UserData.KycData.ApprovedDate = DateTime.UtcNow;
                    SumSubDocsDto docs = await sumsubKycService.GetApplicantDocumentsStatusAsync(webHookEvent.ApplicantId);
                    if (docs.PhoneVerification?.ReviewResult?.Answer == ReviewAnswer.Green)
                    {
                        user.UserData.IsPhoneVerified = true;
                    }
                    await userRepository.Update(user);
                    break;
                case ReviewAnswer.Red:
                    Log.Information($"Applicant {webHookEvent.ApplicantId} has been rejected.");
                    switch (webHookEvent.ReviewResult.RejectType)
                    {
                        case ReviewRejectType.FINAL:
                            user.UserData.KycData.Status = KycStatus.FinalRejected;
                            break;
                        case ReviewRejectType.RETRY:
                            user.UserData.KycData.Status = KycStatus.RetryRejected;
                            break;
                        default:
                            break;
                    }
                    await userRepository.Update(user);
                    break;
                default:
                    break;
            }
        }
        private async Task<UserDto> GetUser(string externalUserId)
        {
            Guid userId;
            if (!Guid.TryParse(externalUserId, out userId))
            {
                throw new ArgumentException("Invalid user ID.");
            }
            var user = await userRepository.GetById(userId);
            if (user is null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            return user;
        }
        private Dictionary<string, List<long>> ProcessImagesTypesAndIds(SumSubDocsDto docs)
        {
            var result = new Dictionary<string, List<long>>();
            foreach (PropertyInfo prop in docs.GetType().GetProperties())
            {
                var docDetail = prop.GetValue(docs) as SumSubDocDetail;
                if (docDetail?.ImageIds is not null && docDetail.IdDocType is not null)
                {
                    _ = result.TryAdd(docDetail.IdDocType, docDetail.ImageIds);
                }
            }
            return result;
        }
        #endregion
    }
}