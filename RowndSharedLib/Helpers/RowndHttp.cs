using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic;
using Polly;
using Polly.Retry;

namespace Rownd.Helpers
{
    public class ResultSet<T>
    {
        [JsonPropertyName("total_results")]
        public int TotalResults { get; set; }

        [JsonPropertyName("results")]
        public T[] Results { get; set; } = Array.Empty<T>();
    }
    public static class RowndHttp
    {
        static ResiliencePipeline defaultPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions())
            .AddTimeout(TimeSpan.FromSeconds(30))
            .Build();

        private static SocketsHttpHandler handler = new()
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15) // Recreate every 15 minutes
        };
        private static HttpClient? defaultClient;

        public static void Initialize(RowndClient rownd)
        {
            if (defaultClient == null)
            {
                if (rownd.Config.IsDebugModeEnabled)
                {
                    defaultClient = new HttpClient(new HttpLoggingHandler(handler));
                }
                else
                {
                    defaultClient = new HttpClient(handler);
                }

                defaultClient.DefaultRequestHeaders.Add("User-Agent", RowndConstants.DefaultUserAgent);
                defaultClient.DefaultRequestHeaders.Add("X-Rownd-App-Key", rownd.Config.AppKey);
                defaultClient.DefaultRequestHeaders.Add("X-Rownd-App-Secret", rownd.Config.AppSecret);
            }
        }

        public static HttpClient GetHttpClient()
        {
            if (defaultClient == null) throw new Exception("RowndHttp has not been initialized. Please call RowndHttp.Initialize() before using RowndHttp.GetHttpClient().");
            return defaultClient;
        }
    }

    internal class HttpLoggingHandler : DelegatingHandler
    {
        public HttpLoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Request:");
            Console.WriteLine(request.ToString());
            if (request.Content != null)
            {
                Console.WriteLine(await request.Content.ReadAsStringAsync());
            }
            Console.WriteLine();

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            Console.WriteLine("Response:");
            Console.WriteLine(response.ToString());
            if (response.Content != null)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            Console.WriteLine();

            return response;
        }
    }
}