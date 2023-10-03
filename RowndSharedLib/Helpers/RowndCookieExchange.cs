using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rownd.Core;
using Rownd.Models;

namespace Rownd.Helpers
{
    public class TokenExchangeResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("should_refresh_page")]
        public bool ShouldRefreshPage { get; set; } = false;

        public TokenExchangeResponse(string msg)
        {
            Message = msg;
        }
    }

    public class SignOutResponse
    {
        [JsonPropertyName("return_to")]
        public string? ReturnTo { get; set; }
    }

    public class TokenExchangeRequest
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }

    [ApiController]
    public abstract class RowndCookieExchange : ControllerBase
    {
        private RowndClient _rowndClient { get; set; }
        private ILogger _logger;

        protected string _signOutRedirectUrl { get; set; } = "/";
        protected string _defaultAuthenticationScheme { get; set; } = IdentityConstants.ApplicationScheme;

        protected bool _addNewUsersToDatabase { get; set; } = true;
        protected UserManager<IdentityUser>? _userManager { get; set; }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected virtual async Task IsAllowedToSignIn(RowndUserProfile rowndUser, Dictionary<string, dynamic> signInContext)
        {
            return;
        }

        protected virtual async Task OnSuccessfulSignIn(RowndUserProfile? rowndUser, IdentityUser? user, Dictionary<string, dynamic> signInContext)
        {
            return;
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        public RowndCookieExchange(RowndClient client, ILogger<RowndCookieExchange> logger)
        {
            _rowndClient = client;
            _logger = logger;
        }

        /**
         * Sign in a user via their Rownd access token
         * Creates a .NET Identity session
         **/
        [Consumes("application/json")]
        [HttpPost]
        public async Task<IActionResult> OnPostAsync([FromBody] TokenExchangeRequest tokenReq)
        {
            if (tokenReq.AccessToken == null)
            {
                return Unauthorized();
            }

            JwtSecurityToken tokenInfo;
            try
            {
                tokenInfo = await _rowndClient.Auth.ValidateToken(tokenReq.AccessToken);
            }
            catch (Exception e)
            {
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

            // Can't proceed if audiences is null
            if (appUserId == null || jwtPayload.Aud == null)
            {
                return Unauthorized("The provided token did not contain the required information: missing app_user_id or audience.");
            }

            var appId = Array.Find(audiences.ToArray(), audience => audience.StartsWith("app:"));

            if (appId != null)
            {
                appId = appId.Split("app:")[1];
            }

            // Map Rownd user profile fields to Identity Framework claims
            var userProfile = new UserClient(_rowndClient);

            RowndUserProfile profile;
            try
            {
                profile = await userProfile.GetProfile(appUserId);
            }
            catch (HttpRequestException e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }

            var profileClaims = new List<Claim>()
            {
                new(ClaimTypes.NameIdentifier, appUserId),
                new("Id", appUserId),
            };

            foreach (KeyValuePair<string, string> entry in _rowndClient.Config.IdentityClaimTypeMap)
            {
                if (profile?.Data?.ContainsKey(entry.Value) ?? false)
                {
                    profileClaims.Add(new Claim(entry.Key, profile?.Data[entry.Value].ToString()));
                }
            }

            Dictionary<string, dynamic>? signInContext = new();
            try
            {
                await IsAllowedToSignIn(profile, signInContext);
            }
            catch (Exception e)
            {
                _logger.LogDebug($"User {appUserId} was prevented from signing in. Reason: {e.Message} {e.StackTrace}");
                return StatusCode(StatusCodes.Status403Forbidden, "You are not permitted to sign in: " + e.Message);
            }

            TokenExchangeResponse response;
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                response = new TokenExchangeResponse("Authentication successful");
                response.ShouldRefreshPage = true;
            }
            else
            {
                response = new TokenExchangeResponse("Already authenticated");
                return Ok(response);
            }

            IdentityUser? user = null;
            if (_userManager != null)
            {
                user = await _userManager.FindByIdAsync(appUserId);
                if (_addNewUsersToDatabase && user == null)
                {
                    user = new IdentityUser()
                    {
                        Id = appUserId,
                        UserName = appUserId,
                        Email = (profile?.Data?.ContainsKey("email") ?? false) ? profile.Data["email"].ToString() : null,
                        PhoneNumber = (profile?.Data?.ContainsKey("phone_number") ?? false) ? profile.Data["phone_number"].ToString() : null,
                    };
                    var result = await _userManager.CreateAsync(user);

                    if (!result.Succeeded)
                    {
                        _logger.LogError("failed to create a new user:", result.Errors);
                    }
                }

                if (user != null)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    foreach (string role in userRoles)
                    {
                        profileClaims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }
            }

            _logger.LogDebug($"Signing in user (claims: {string.Join(", ", profileClaims)})");
            ClaimsIdentity identity = new(profileClaims, _defaultAuthenticationScheme);

            await HttpContext.SignInAsync(
                _defaultAuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProperties
            );

            try {
                await OnSuccessfulSignIn(profile, user, signInContext);
            } catch (Exception e) {
                _logger.LogDebug($"Post-sign-in hook failed for user: {appUserId} . Reason: {e.Message} {e.StackTrace}");
            }

            return Ok(response);
        }

        /**
         * Sign out the current user
         **/
        [Consumes("application/json")]
        [HttpDelete]
        public async Task<IActionResult> OnDeleteAsync()
        {
            var authProperties = new AuthenticationProperties()
            {
                RedirectUri = _signOutRedirectUrl
            };

            await HttpContext.SignOutAsync(
                _defaultAuthenticationScheme,
                authProperties
            );

            var response = new SignOutResponse
            {
                ReturnTo = _signOutRedirectUrl
            };

            return Ok(response);
        }
    }
}

