using System.Security.Claims;

namespace Rownd.Models {
    public class Config {
        public string ApiUrl { get; set; } = "https://api.rownd.io";
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public Dictionary<string, string> IdentityClaimTypeMap { get; set; } = new Dictionary<string, string>()
        {
            { ClaimTypes.GivenName, "first_name" },
            { ClaimTypes.Surname, "last_name" },
            { ClaimTypes.Email, "email" },
            { ClaimTypes.Role, "role" },
            { ClaimTypes.MobilePhone, "phone_number" },
            { ClaimTypes.Name, "first_name" }
        };

        public bool IsDebugModeEnabled { get; set; } = false;

        public Config() {
            AppKey = Environment.GetEnvironmentVariable("ROWND_APP_KEY");
            AppSecret = Environment.GetEnvironmentVariable("ROWND_APP_SECRET");

            if (AppKey == null || AppSecret == null) {
                throw new Exception("Missing environment variables: ROWND_APP_KEY and ROWND_APP_SECRET. Ensure they are set or pass them directly via new Config(appKey, appSecret).");
            }
        }

        public Config(string appKey, string appSecret) {
            AppKey = appKey;
            AppSecret = appSecret;
        }
    }
}