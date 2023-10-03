using System.Text.Encodings.Web;
using System.Text.Json;
using Rownd.Helpers;
using Rownd.Models;

namespace Rownd.Core
{
    public static class AppConfigClient
    {
        public static async Task<AppConfig> FetchAppConfig(RowndClient rowndClient)
        {
            var httpClient = RowndHttp.GetHttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{rowndClient.Config.ApiUrl}/hub/app-config"),
                Method = HttpMethod.Get,
            };

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var strContent = await response.Content.ReadAsStringAsync();
            var jsonContent = JsonSerializer.Deserialize<AppConfigWrapper>(strContent, new JsonSerializerOptions() {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true,
            });

            var content = await response.Content.ReadFromJsonAsync<AppConfigWrapper>(new JsonSerializerOptions() {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true,
            });

            Console.WriteLine(content);

            return content?.App ?? throw new RowndException($"The app config could not be retrieved.");
        }
    }
}