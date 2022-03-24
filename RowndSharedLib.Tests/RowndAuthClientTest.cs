using Xunit;
using Rownd.Core;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System;
using System.IdentityModel.Tokens.Jwt;
using Rownd.Models;

namespace RowndSharedLib.Tests
{

    public class RowndAuthClientTest
    {

        Config _config;

        public RowndAuthClientTest()
        {
            _config = new Config("49f4c15a-956a-4293-9e3f-f9746f02dd1e", "3036e37a952d6a6bea5a1d4487d224e820dda371e7684a50");
            _config.ApiUrl = "https://api.us-east-2.dev.rownd.io";
        }

        [Fact]
        public async Task RejectProperlySignedButExpiredJwt()
        {
            var rownd = new AuthClient(_config);

            try
            {
                var result = await rownd.ValidateToken("eyJhbGciOiJFZERTQSJ9.eyJqdGkiOiIyYzZiNTg4Ny04NzIyLTQyN2YtOGI4ZC0wMmEwNjg1NDYwMDYiLCJhdWQiOlsiYXBwOjI5MDE2NzI4MTczMjgxMzMxNSIsImh0dHBzOi8vYXBpLmRldi5yb3duZC5pbyJdLCJzdWIiOiJyb3duZHw0NTE1ZTNiMTlmZmQzNTVmOTEyOSIsImlhdCI6MTY0NzU3MjY2NiwiaHR0cHM6Ly9hdXRoLnJvd25kLmlvL2FwcF91c2VyX2lkIjoiZWJjNTM1MWUtNjMyYi00MDExLWJiYjQtZWNkMTc2MzAxNTM1IiwiaHR0cHM6Ly9hdXRoLnJvd25kLmlvL2lzX3ZlcmlmaWVkX3VzZXIiOnRydWUsImlzcyI6Imh0dHBzOi8vYXBpLmRldi5yb3duZC5pbyIsImV4cCI6MTY0NzU3NjI2Nn0.GGcbcnu2yd0gh1MjapaFy1Ly1V8H4JOHxSw02oAjoBZ1RJsM3gqvHH-y5Q6GH9nkTFBsC7oPIZtO-xX7qsEEBg");

                // Assert.IsType<JwtSecurityToken>(result.Result);
            }
            catch (Exception e)
            {
                Assert.IsType<SecurityTokenExpiredException>(e);
            }
        }

        [Fact]
        public async Task RejectImproperlySignedJwt()
        {
            var rownd = new AuthClient(_config);
            try
            {
                await rownd.ValidateToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");
            }
            catch (Exception e)
            {
                Assert.IsType<SecurityTokenSignatureKeyNotFoundException>(e);
            }
        }
    }
}