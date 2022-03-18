using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Rownd.Models;

namespace Rownd.Helpers
{
    public class RowndAuthHandler : AuthenticationHandler<RowndAuthOptions>
    {
        private RowndClient _rowndClient { get; set; }

        public RowndAuthHandler(
            IOptionsMonitor<RowndAuthOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            RowndClient client
            )
            : base(options, logger, encoder, clock)
        {
            _rowndClient = client;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Options.ErrOnInvalidToken && !Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            string authorizationHeader = Request.Headers["Authorization"];
            if (Options.ErrOnInvalidToken && string.IsNullOrEmpty(authorizationHeader))
            {
                return AuthenticateResult.NoResult();
            }

            if (!authorizationHeader.StartsWith(RowndAuthenticationDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            string token = authorizationHeader.Substring(RowndAuthenticationDefaults.AuthenticationScheme.Length).Trim();

            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            var tokenInfo = await _rowndClient.Auth.ValidateToken(token);
            var jwtPayload = tokenInfo.Payload;

            var claims = jwtPayload.Claims;

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}