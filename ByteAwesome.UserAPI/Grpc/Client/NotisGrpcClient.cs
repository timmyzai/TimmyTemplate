using ByteAwesome.NotificationAPI.Grpc;

namespace ByteAwesome.UserAPI.GrpcClient
{
    public interface INotisGrpcClient
    {
        Task SendEmail(string receiverName, string receiverEmail, string emailSubject, string emailContent, string languageCode = "en");
        Task SendSMS(string receiverPhoneNumber, string smsSubject, string smsContent, string languageCode = "en");
    }
    public class NotisGrpcClient : INotisGrpcClient
    {
        private readonly NotisGrpcService.NotisGrpcServiceClient client;

        public NotisGrpcClient(NotisGrpcService.NotisGrpcServiceClient client)
        {
            this.client = client;
        }
        public async Task SendEmail(string receiverName, string receiverEmail, string emailSubject, string emailContent, string languageCode = "en")
        {
            try
            {
                var emailDto = new SendEmailRequest()
                {
                    ReceiverName = receiverName,
                    ReceiverEmail = receiverEmail,
                    Subject = emailSubject,
                    Content = emailContent,
                    LanguageCode = languageCode
                };
                var response = await client.SendEmailAsync(emailDto);
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage, fireAndForget: true);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
            }
        }
        public async Task SendSMS(string receiverPhoneNumber, string smsSubject, string smsContent, string languageCode = "en")
        {
            try
            {
                var phoneDto = new SendSMSRequest()
                {
                    ReceiverPhone = receiverPhoneNumber,
                    Subject = smsSubject,
                    Content = smsContent,
                    LanguageCode = languageCode
                };

                var response = await client.SendSMSAsync(phoneDto);
                GrpcResponseValidator.ValidateResponse(response.ErrorMessage, fireAndForget: true);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
            }
        }
    }
}
