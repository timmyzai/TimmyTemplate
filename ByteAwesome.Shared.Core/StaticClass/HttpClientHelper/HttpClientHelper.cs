namespace ByteAwesome
{
    public class HttpClientHelper
    {
        public static void AddHeader(HttpClient client, Dictionary<string, string> headers)
        {
            headers?.ToList().ForEach(header =>
            {
                if (!client.DefaultRequestHeaders.Contains(header.Key))
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            });
        }
    }
}