using System.Text.Json.Serialization;
using Rownd.Helpers;

namespace Rownd.Models {
    public class AppConfig {
        [JsonPropertyName("id")]
        public string? Id;
    }

    internal class AppConfigWrapper {
        [JsonPropertyName("app")]
        public AppConfig? App { get; set; }
    }
}