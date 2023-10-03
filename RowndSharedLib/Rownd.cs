using Rownd.Core;
using Rownd.Helpers;
using Rownd.Models;

namespace Rownd {
    public class RowndClient
    {
        public readonly Config Config;
        public readonly AuthClient Auth;

        public readonly UserClient Users;

        public AppConfig? AppConfig { get; private set; }

        public RowndClient(Config config, ILogger<RowndClient> logger) {
            Config = config;
            RowndHttp.Initialize(this);
            Auth = new AuthClient(Config);
            Users = new UserClient(this);

            Task.Run(async () => {
                AppConfig = await AppConfigClient.FetchAppConfig(this);
                Console.WriteLine(AppConfig);
            }).Wait();
        }

        public RowndClient(string appKey, string appSecret, ILogger<RowndClient> logger) : this(new Config(appKey, appSecret), logger) {}

        public RowndClient(ILogger<RowndClient> logger) : this(new Config(), logger) {}
    }
}