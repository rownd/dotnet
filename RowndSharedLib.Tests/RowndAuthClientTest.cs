using Xunit;
using Rownd.Core;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace RowndSharedLib.Tests;

public class RowndAuthClientTest
{
    [Fact]
    public void ValidateGoodJwt()
    {
        var rownd = new AuthClient();
        var result = rownd.ValidateToken("eyJhbGciOiJFZERTQSJ9.eyJqdGkiOiIyZDBmMmRmMy05NjdlLTQzYjQtOGFkYS0xNjczZDM3NzY5MWUiLCJhdWQiOlsiYXBwOjI5MDE2NzI4MTczMjgxMzMxNSIsImh0dHBzOi8vYXBpLmRldi5yb3duZC5pbyJdLCJzdWIiOiJyb3duZHw0NTE1ZTNiMTlmZmQzNTVmOTEyOSIsImlhdCI6MTY0NzUzMTc3MiwiaHR0cHM6Ly9hdXRoLnJvd25kLmlvL2FwcF91c2VyX2lkIjoiZWJjNTM1MWUtNjMyYi00MDExLWJiYjQtZWNkMTc2MzAxNTM1IiwiaHR0cHM6Ly9hdXRoLnJvd25kLmlvL2lzX3ZlcmlmaWVkX3VzZXIiOnRydWUsImlzcyI6Imh0dHBzOi8vYXBpLmRldi5yb3duZC5pbyIsImV4cCI6MTY0NzUzNTM3Mn0.yy5NGHGfSJVQMvv4GDe2G0C59U7pQbnWU6cW7f4pPvtnpNA-uHKWZz4NtKJgeIUKJrxOXLXCmoX9xU2g9KBCCA");

        Assert.IsType<JwtSecurityToken>(result.Result);
    }

    [Fact]
    public async Task RejectBadJwt()
    {
        var rownd = new AuthClient();
            try {
                await rownd.ValidateToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");
            } catch (Exception e) {
                Assert.IsType<SecurityTokenSignatureKeyNotFoundException>(e);
            }
    }
}