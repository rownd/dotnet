using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rownd.Core;
using Rownd.Models;

namespace Rownd.Helpers
{
	public class TokenExchangeResponse
    {
		public string? message { get; set; }
		public bool should_refresh_page { get; set; } = false;

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
	public abstract class RowndCookieExchange : ControllerBase
	{
		private RowndClient _rowndClient { get; set; }
		private ILogger _logger;

		protected bool _addNewUsersToDatabase { get; set; } = false;

		protected UserManager<IdentityUser>? _userManager { get; set; }

		public RowndCookieExchange(RowndClient client, ILogger<RowndCookieExchange> logger)
		{
			_rowndClient = client;
			_logger = logger;
		}

		[Consumes("application/json")]
		[HttpPost]
		public async Task<IActionResult> OnPostAsync([FromBody] TokenExchangeRequest tokenReq)
        {
			if (tokenReq.access_token == null) {
				return Unauthorized();
			}

			JwtSecurityToken tokenInfo;
			try {
				tokenInfo = await _rowndClient.Auth.ValidateToken(tokenReq.access_token);
			} catch (Exception e) {
				_logger.LogError(e, "Failed to validate token");
				return Unauthorized();
			}

			var jwtPayload = tokenInfo.Payload;

			var authProperties = new AuthenticationProperties
			{
				IsPersistent = true
			};


            string appUserId = jwtPayload["https://auth.rownd.io/app_user_id"] as string;
			IList<string> audiences = jwtPayload.Aud;

			_logger.LogDebug($"Rownd appUserId: {appUserId}");
			_logger.LogDebug($"Rownd token audiences: {jwtPayload.Aud}");

			ClaimsIdentity identity;
            // Can't proceed if audiences is null
            if (appUserId == null || jwtPayload.Aud == null)
            {
				return Unauthorized("The provided token did not contain the required information: missing app_user_id or audience.");
			}
                
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

			_logger.LogDebug($"Signing in with identity: {String.Join(", ", profileClaims)} claims");
			identity = new ClaimsIdentity(profileClaims, "Identity.Application");

			var response = new TokenExchangeResponse("Authentication successful");
			if (User?.Identity == null || !User.Identity.IsAuthenticated) {
				response.should_refresh_page = true;
			}

			await HttpContext.SignInAsync(
				"Identity.Application",
				new ClaimsPrincipal(identity),
				authProperties
			);

			if (_addNewUsersToDatabase && _userManager != null) {
				var existingUser = await _userManager.FindByIdAsync(appUserId);
				if (existingUser == null) {
					var newUser = new IdentityUser() {
						Id = appUserId,
						UserName = appUserId,
						Email = (profile?.data?.ContainsKey("email") ?? false) ? profile.data["email"].ToString() : null,
						PhoneNumber = (profile?.data?.ContainsKey("phone_number") ?? false) ? profile.data["phone_number"].ToString() : null,
					};
					var result = await _userManager.CreateAsync(newUser);

					if (!result.Succeeded)
                    {
						_logger.LogError("failed to create a new user:", result.Errors);
                    }
				}
			}

			return Ok(response);
		}
	}
}

