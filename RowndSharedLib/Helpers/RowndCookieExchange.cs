using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Rownd;
using Rownd.Core;
using Rownd.Models;

namespace RowndSharedLib.Helpers
{
	public class TokenExchangeResponse
    {
		public string? message { get; set; }

		public TokenExchangeResponse(string msg)
        {
			message = msg;
        }
    }

	public class TokenExchangeRequest
    {
		public string? access_token { get; set; }
    }

	[ApiController]
	public class RowndCookieExchange : ControllerBase
	{
		private RowndClient _rowndClient { get; set; }

		public RowndCookieExchange(RowndClient client)
		{
			_rowndClient = client;
		}

		[Consumes("application/json")]
		[HttpPost]
		public async Task<IActionResult> OnPostAsync([FromBody] TokenExchangeRequest tokenReq)
        {
			var tokenInfo = await _rowndClient.Auth.ValidateToken(tokenReq.access_token);
			var jwtPayload = tokenInfo.Payload;

			var authProperties = new AuthenticationProperties
			{
				IsPersistent = true
			};


            string appUserId = jwtPayload["https://auth.rownd.io/app_user_id"] as string;
			IList<string> audiences = jwtPayload.Aud;

			ClaimsIdentity identity;
            // Can't proceed if audiences is null
            if (appUserId != null && jwtPayload.Aud != null)
            {
                var appId = Array.Find<string>(audiences.ToArray<string>(), audience => audience.StartsWith("app:"));

                if (appId != null)
                {
                    appId = appId.Split("app:")[1];
                }

                // Map Rownd user profile fields to Identity Framework claims
                var userProfile = new UserProfile(appId, appUserId, _rowndClient.Config);

				RowndUser profile;
				try
				{
					profile = await userProfile.GetProfile();
				}
				catch (HttpRequestException e)
                {
					return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
                }

				var profileClaims = new List<Claim>()
				{
					new Claim(ClaimTypes.NameIdentifier, appUserId),
                };

                foreach (KeyValuePair<string, string> entry in _rowndClient.Config.IdentityClaimTypeMap)
                {
                    if (profile?.data?.ContainsKey(entry.Value) ?? false)
                    {
                        profileClaims.Add(new Claim(entry.Key, profile?.data[entry.Value].ToString()));
                    }
                }

				identity = new ClaimsIdentity(profileClaims, "Identity.Application");
			}
			else
            {
				identity = new ClaimsIdentity(jwtPayload.Claims, "Identity.Application");
            }

			await HttpContext.SignInAsync(
				"Identity.Application",
				new ClaimsPrincipal(identity),
				authProperties
			);

			return Ok(new TokenExchangeResponse("Authentication successful"));
		}
	}
}

