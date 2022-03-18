using Rownd.Core;
using Rownd.Models;

namespace Rownd {
    public class RowndClient
    {
        private readonly Config _config;
        public readonly AuthClient Auth;

        public RowndClient(Config config, AuthClient authClient) {
            _config = config;
            Auth = authClient;
        }

        public RowndClient(string appKey, string appSecret)
        {
            _config = new Config(appKey, appSecret);
            Auth = new AuthClient(_config);
        }

        public RowndClient() {
            _config = new Config();
            Auth = new AuthClient(_config);
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