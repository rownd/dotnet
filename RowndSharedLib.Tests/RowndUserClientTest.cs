using Xunit;
using Rownd.Core;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System;
using System.IdentityModel.Tokens.Jwt;
using Rownd.Models;
using Rownd;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;
using System.Collections.Generic;

namespace RowndSharedLib.Tests
{

    public class RowndUserClientTest
    {
        private readonly ITestOutputHelper output;
        Config _config;
        RowndClient _rownd;

        public RowndUserClientTest(ITestOutputHelper output)
        {
            this.output = output;

            _config = new Config("49f4c15a-956a-4293-9e3f-f9746f02dd1e", "3036e37a952d6a6bea5a1d4487d224e820dda371e7684a50")
            {
                ApiUrl = "https://api.us-east-2.dev.rownd.io"
            };
            _rownd = new RowndClient(_config, new NullLogger<RowndClient>());
        }

        [Fact]
        public async Task TestListUsers()
        {
            var result = await _rownd.Users.ListProfiles(new UserLookupOpts
            {
                UserLookupCriteria = new string[] { "dotnet@rownd.app" }
            });
            output.WriteLine($"total results: {result.TotalResults}");
            Assert.True(result.TotalResults == 1);
        }

        [Fact]
        public async Task TestDeleteUser()
        {
            Random rnd = new Random();
            var email = $"dotnet_test{rnd.Next(1000, 2000)}@rownd.app";
            var createResult = await _rownd.Users.UpdateProfile(new RowndUserProfile() {
                Data = new Dictionary<string, dynamic>() {
                    { "email", email },
                    { "user_id", "__default__" }
                }
            });

            Assert.NotNull(createResult.Id);
            Assert.True(createResult.Id != "__default__");
            Assert.True(Convert.ToString(createResult?.Data?["email"]) == email);

            await _rownd.Users.DeleteProfile(createResult.Id);
        }
    }
}