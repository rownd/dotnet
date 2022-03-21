using Rownd.Core;
using Rownd.Models;

namespace Rownd {
    public class RowndClient
    {
        public readonly Config Config;
        public readonly AuthClient Auth;

        public RowndClient(Config config, AuthClient authClient) {
            Config = config;
            Auth = authClient;
        }

        public RowndClient(string appKey, string appSecret)
        {
            Config = new Config(appKey, appSecret);
            Auth = new AuthClient(Config);
        }

        public RowndClient() {
            Config = new Config();
            Auth = new AuthClient(Config);
        }

        public async Task Authenticate() {

        }

        // public async Task<RowndUser> GetUser(string token) {
        //     var user = await authClient.ValidateToken(token);
        //     return new RowndUser {
        //         id = user.Id,
        //         data = user.Data
        //     };
        // }
    }
}