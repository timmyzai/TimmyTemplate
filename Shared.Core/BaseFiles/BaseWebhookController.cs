using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AwesomeProject
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("webhooks/[controller]")]
    public abstract class BaseWebHookController : BaseController
    {
        private const string DefaultWebHookEndPoint = "Event";
        private readonly Dictionary<string, Func<string, Task>> _webhookEventHandlers = [];
        protected BaseWebHookController()
        {
            InitializeEventHandlers();
        }
        protected void RegisterWebhookEventHandler(string eventName, Func<string, Task> handler) =>
            _webhookEventHandlers[eventName] = handler;

        protected virtual void InitializeEventHandlers() =>
            RegisterWebhookEventHandler(DefaultWebHookEndPoint, HandleWebHookEvent);

        protected virtual Task HandleWebHookEvent(string webHookRawText) =>
            throw new NotImplementedException();

        [HttpPost("{eventName}")]
        public async Task<IActionResult> Event(string eventName, [FromBody] JsonElement webHookEventJson)
        {
            try
            {
                if (!_webhookEventHandlers.TryGetValue(eventName, out var handler))
                    throw new Exception($"Unsupported WebHook Event: {eventName}");

                if (webHookEventJson.ValueKind is not JsonValueKind.Object)
                    throw new Exception("Invalid WebHook Event.");

                var webHookRawText = webHookEventJson.GetRawText();
                Log.Information("{EventName} - Start processing. JSON: {Json}", eventName, webHookRawText);
                await handler(webHookRawText);
                return Ok(new { Message = "Successfully Handled" });
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                return Ok(new { Message = "Handled with Error." });
            }
        }
    }
}
