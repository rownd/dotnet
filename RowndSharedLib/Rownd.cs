using Rownd.Core;
using Rownd.Helpers;
using Rownd.Models;

namespace Rownd {
    public class RowndClient
    {
        public readonly Config Config;
        public readonly AuthClient Auth;

        public RowndClient(Config config, ILogger<RowndClient> logger) {
            Config = config;
            Auth = new AuthClient(Config);
        }

        public RowndClient(string appKey, string appSecret, ILogger<RowndClient> logger)
        {
            Config = new Config(appKey, appSecret);
            Auth = new AuthClient(Config);
        }

        public RowndClient(ILogger<RowndClient> logger) {
            Config = new Config();
            Auth = new AuthClient(Config);
        }
    }
}