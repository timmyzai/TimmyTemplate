namespace ByteAwesome
{
    public class HttpClientPolicySettingsDto
    {
        public int RetryCount { get; set; } = 3;
        public int EventsAllowedBeforeBreak { get; set; } = 5;
        public int DurationOfBreakInSeconds { get; set; } = 30;
        public int TimeoutInSeconds { get; set; } = 10;
        public HttpClientPolicySettingsDto() { }
        public HttpClientPolicySettingsDto(int retryCount, int eventsAllowedBeforeBreak, int durationOfBreakInSeconds, int timeoutInSeconds)
        {
            RetryCount = retryCount;
            EventsAllowedBeforeBreak = eventsAllowedBeforeBreak;
            DurationOfBreakInSeconds = durationOfBreakInSeconds;
            TimeoutInSeconds = timeoutInSeconds;
        }
    }
}