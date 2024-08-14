using System.ComponentModel;
using ByteAwesome.UserAPI.ExternalApis.SumsubKycService.Dto;
using Newtonsoft.Json;

namespace ByteAwesome.UserAPI.Models.WebHook.Dtos
{
    public class SumsubWebHookEventDto
    {
        [JsonProperty("applicantId")]
        public string ApplicantId { get; set; }

        [JsonProperty("inspectionId")]
        public string InspectionId { get; set; }

        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("levelName")]
        public string LevelName { get; set; }

        [JsonProperty("externalUserId")]
        public string ExternalUserId { get; set; }

        private string _type;
        [JsonProperty("type")]
        public string Type
        {
            get => _type;
            set => _type = GeneralHelper.SetEnum(value, out _eventType);
        }
        private SumsubEventType? _eventType;
        [JsonIgnore]
        public SumsubEventType? EventType => _eventType;

        [JsonProperty("sandboxMode")]
        public bool SandboxMode { get; set; }

        private string _reviewStatus;
        [JsonProperty("reviewStatus")]
        public string ReviewStatus
        {
            get => _reviewStatus;
            set => _reviewStatus = GeneralHelper.SetEnum(value, out Status);
        }
        [JsonIgnore]
        public ReviewStatus? Status;

        [JsonProperty("createdAtMs")]
        public string CreatedAtMs { get; set; }

        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("reviewResult")]
        public ReviewResult ReviewResult { get; set; }

        [JsonProperty("applicantType")]
        public string ApplicantType { get; set; }

        [JsonProperty("applicantActionId")]
        public string ApplicantActionId { get; set; }

        [JsonProperty("externalApplicantActionId")]
        public string ExternalApplicantActionId { get; set; }
    }

    public class ReviewResult
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
        [JsonProperty("rejectLabels")]
        public List<string> RejectLabels { get; set; }
        private string _reviewRejectType;
        [JsonProperty("reviewRejectType")]
        public string ReviewRejectType
        {
            get => _reviewRejectType;
            set => _reviewRejectType = GeneralHelper.SetEnum(value, out RejectType);
        }
        [JsonIgnore]
        public ReviewRejectType? RejectType;
        [JsonProperty("clientComment")]
        public string ClientComment { get; set; }

        [JsonProperty("moderationComment")]
        public string ModerationComment { get; set; }

        [JsonProperty("buttonIds")]
        public List<string> ButtonIds { get; set; }
    }
    public enum ReviewRejectType
    {
        [Description("Final reject, e.g. when a person is a fraudster, or a client does not want to accept such kinds of clients in his/her system")]
        FINAL,

        [Description("Decline that can be fixed, e.g. by uploading an image of better quality")]
        RETRY
    }
    public enum ReviewStatus
    {
        [Description("Initial registration has started. A client is still in the process of filling out the applicant profile. Not all required documents are currently uploaded.")]
        Init,

        [Description("An applicant is ready to be processed.")]
        Pending,

        [Description("The check is in a half way of being finished.")]
        Prechecked,

        [Description("The checks have been started for the applicant.")]
        Queued,

        [Description("The check has been completed.")]
        Completed,

        [Description("Applicant waits for a final decision from compliance officer (manual check was initiated) or waits for all beneficiaries to pass KYC in case of company verification.")]
        OnHold
    }
    public enum SumsubEventType
    {
        [Description("When an applicant is created.")]
        ApplicantCreated,
        [Description("When a user uploaded all the required documents and the applicant's status changed to pending.")]
        ApplicantPending,
        [Description("When verification is completed. Contains the verification result. More information about this type of webhook can be found here.")]
        ApplicantReviewed,
        [Description("Processing of the applicant is paused for an agreed reason.")]
        ApplicantOnHold,
        [Description("Applicant action status changed to pending. More info about applicant actions you may find here.")]
        ApplicantActionPending,
        [Description("Applicant action verification has been completed. More info about applicant actions you may find here.")]
        ApplicantActionReviewed,
        [Description("Applicant action verification has been paused for an agreed reason. More info about applicant actions you may find here.")]
        ApplicantActionOnHold,
        [Description("Applicant's personal info has been changed.")]
        ApplicantPersonalInfoChanged,
        [Description("Applicant has been permanently deleted.")]
        ApplicantDeleted,
        [Description("Applicant has been reset: applicant status changed to init and all documents were set as inactive. You can find more info here.")]
        ApplicantReset,
        [Description("Applicant level has been changed.")]
        ApplicantLevelChanged,
        [Description("Workflow has been completed for an applicant.")]
        ApplicantWorkflowCompleted,
    }
}
