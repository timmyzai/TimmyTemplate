using UserAPI.Modules;
using UserAPI.Models.Dtos;
using Newtonsoft.Json;
using System.Text;

namespace UserAPI.Helper
{
    public static class EmailHelper
    {
        private static BrevoMailModuleConfig _brevoMailConfig;

        public static void Configure(BrevoMailModuleConfig brevoMailConfig)
        {
            _brevoMailConfig = brevoMailConfig;
        }
        public static async Task SendEmail(SendEmailDTO input)
        {
            await BrevoMailService(input);
        }
        private static async Task BrevoMailService(SendEmailDTO input)
        {
            if (_brevoMailConfig.IsEnabled == false) return;
            string url = _brevoMailConfig.URL;
            string apiKey = _brevoMailConfig.apiKey;
            string ownerEmail = _brevoMailConfig.ownerEmail;
            const string sender = "Bytebank";

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

            var jsonObject = new
            {
                sender = new { email = ownerEmail, name = sender },
                to = new[] { new { email = input.ReceiverEmail, name = input.ReceiverName } },
                subject = input.Subject,
                htmlContent = input.Content
            };
            var jsonPayload = JsonConvert.SerializeObject(jsonObject);
            var result = await httpClient.PostAsync(url, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

            if (!result.IsSuccessStatusCode)
            {
                var resultContent = await result.Content.ReadAsStringAsync();
                throw new Exception($"Brevo Mail Response: {resultContent}");
            }
        }
    }
}