using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace AwesomeProject
{
    public static class HttpClientHelper
    {
        public static HttpRequestMessage CreateRequestMessage(HttpMethod method, string path, string jsonBody = null)
        {
            HttpContent content = jsonBody is not null ? new StringContent(jsonBody, Encoding.UTF8, "application/json") : null;
            var request = new HttpRequestMessage(method, path) { Content = content };
            return request;
        }
        public static void AddHeader(HttpRequestMessage request, Dictionary<string, string> headers)
        {
            if (headers is null) return;
            foreach (var header in headers)
            {
                if (!request.Headers.Contains(header.Key))
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
        }
        public static async Task<T> SendRequestAsync<T>(HttpClient client, HttpRequestMessage request)
        {
            T result = default;
            try
            {
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                result = await ProcessResponse<T>(response);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "HTTP request exception: {Method} {Uri}", request.Method, request.RequestUri);
            }
            return result;
        }
        public static async Task SendRequestAsync(HttpClient client, HttpRequestMessage request)
        {
            try
            {
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "HTTP request exception: {Method} {Uri}", request.Method, request.RequestUri);
            }
        }
        public static async Task<(T Response, Err Error)> SendRequestAsync<T, Err>(HttpClient client, HttpRequestMessage request)
        {
            (T Response, Err Error) result = default;
            HttpResponseMessage response = null;
            try
            {
                response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                response.EnsureSuccessStatusCode();
                result.Response = await ProcessResponse<T>(response);
            }
            catch (HttpRequestException ex)
            {
                result.Error = await ProcessResponse<Err>(response);
                Log.Error(ex, "HTTP request exception: {Method} {Uri}", request.Method, request.RequestUri);
            }
            finally
            {
                response?.Dispose();
            }
            return result;
        }
        private static async Task<T> ProcessResponse<T>(HttpResponseMessage response)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            Log.Debug("Received response: {Content}", responseContent);

            if (responseContent == "Fallback response")
            {
                if (typeof(T) == typeof(byte[]))
                {
                    return (T)(object)new byte[0]; // Return an empty byte array for fallback.
                }
                else
                {
                    return Activator.CreateInstance<T>(); // Fallback for other types.
                }
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)responseContent;
            }
            else if (typeof(T) == typeof(byte[]))
            {
                return (T)(object)await response.Content.ReadAsByteArrayAsync(); // Directly read the byte array from the response.
            }
            else if (typeof(T) == typeof(JObject))
            {
                return (T)(object)JObject.Parse(responseContent);
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(responseContent); // Handle other types through JSON deserialization.
            }
        }

    }
}
