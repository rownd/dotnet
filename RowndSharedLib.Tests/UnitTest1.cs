using Xunit;
using Rownd.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace RowndSharedLib.Tests;

public class UnitTest1
{
    [Fact]
    public void ValidateGoodJwt()
    {
        var rownd = new RowndTokens();
        var result = rownd.ValidateToken("eyJhbGciOiJFZERTQSIsImtpZCI6InNpZy0xNjQ0ODU5MTUwIn0.eyJqdGkiOiJkMWI3ZjNlMi1lNzdlLTQ2MjMtOWIyZC03OGNhNmQwNTgyMTUiLCJhdWQiOlsiYXBwOjMyMTk3NDM4NTU4Mzg1MDA2NCIsImh0dHBzOi8vYXBpLmRldi5yb3duZC5pbyJdLCJzdWIiOiJhdXRoMHw2MWYzMDUzMjUxZjI0MjAwNjk0NTU5NzYiLCJodHRwczovL2F1dGgucm93bmQuaW8vYXBwX3VzZXJfaWQiOiJlM2FjMGEyNy00ZGJkLTQ4MWItYTY1MS04MTI1NjBiZTQ4MTUiLCJodHRwczovL2F1dGgucm93bmQuaW8vaXNfdmVyaWZpZWRfdXNlciI6dHJ1ZSwiaXNzIjoiaHR0cHM6Ly9hcGkuZGV2LnJvd25kLmlvIiwiaWF0IjoxNjQ3NDY0MDM3LCJodHRwczovL2F1dGgucm93bmQuaW8vand0X3R5cGUiOiJyZWZyZXNoX3Rva2VuIiwiZXhwIjoxNjUwMDU2MDM3fQ.U2x-vwACR1B_1p60_7AdWFXXaBuQ76SM76cGlCBNY-JV-Tb3UbjvpnrtMHB5cCmpEuNiRE06fuMihikca4pQCQ");

        Assert.IsType<JwtSecurityToken>(result.Result);
        // Assert.Equal(true, result.Result.IsValid);
    }

    [Fact]
    public async Task RejectBadJwt()
    {
        var rownd = new RowndTokens();
        Func<Task> act = () => rownd.ValidateToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");
        await Assert.ThrowsAnyAsync<ArgumentException>(act);
        // var result = rownd.ValidateToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");
        // Assert.Equal(false, result.Result.IsValid);
    }
}