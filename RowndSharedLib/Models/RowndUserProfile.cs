using System.Text.Json.Serialization;

namespace Rownd.Models {
    public class RowndUserProfile {
        public string? Id {
            get {
                return Data?["user_id"].ToString();
            }
        }

        [JsonPropertyName("data")]
        public Dictionary<string, dynamic>? Data { get; set; }

        [JsonPropertyName("meta")]
        public Dictionary<string, dynamic>? Meta { get; set; }
    }
}