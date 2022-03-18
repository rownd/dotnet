using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using IdentityModel.Client;
using ScottBrady.IdentityModel.Tokens;
using Microsoft.IdentityModel.Logging;
using System.Text.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Newtonsoft.Json;
using Rownd.Models;

namespace Rownd.Core
{

    public class JWKObject
    {
        public string? kty { get; set; }
        public string? use { get; set; }
        public string? kid { get; set; }
        public string? crv { get; set; }
        public string? x { get; set; }
        public string? alg { get; set; }
    }
    public class JWKSetObject
    {
        public JWKObject[]? keys { get; set; }
    }

    public class AuthClient
    {
        private readonly IMemoryCache _memoryCache;
        private readonly Config _config;

        public AuthClient(Config config)
        {
            _config = config;
            var cacheOptions = new MemoryCacheOptions();
            _memoryCache = new MemoryCache(cacheOptions);
            IdentityModelEventSource.ShowPII = true;
        }

        public AuthClient(Config config, IMemoryCache memoryCache)
        {
            _config = config;
            _memoryCache = memoryCache;
        }

        private async Task<List<SecurityKey>> FetchRowndJWKs()
        {

            if (!_memoryCache.TryGetValue("rownd_jwks", out List<SecurityKey> jwks))
            {
                using (var httpClient = new HttpClient())
                {
                    var json = await httpClient.GetStringAsync($"{_config.ApiUrl}/hub/auth/keys");

                    var keySet = System.Text.Json.JsonSerializer.Deserialize<JWKSetObject>(json);

                    jwks = new List<SecurityKey>();
                    if (keySet?.keys == null) {
                        throw new KeyNotFoundException("No keys found in JWKSet. Cannot continue.");
                    }

                    foreach (var jwk in keySet.keys) {
                        if (jwk.alg == "EdDSA") {
                            var jwkBytes = Base64UrlEncoder.DecodeBytes(jwk.x);
                            jwks.Add(new EdDsaSecurityKey(new Ed25519PublicKeyParameters(jwkBytes, 0)));
                        } else {
                            jwks.Add(new JsonWebKey(System.Text.Json.JsonSerializer.Serialize(jwk)));
                        }
                    }

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                    _memoryCache.Set("rownd_jwks", jwks);
                }
            }

            return jwks;
        }

        public async Task<JwtSecurityToken> ValidateToken(string token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("Token is null");
            }

            var jwks = await FetchRowndJWKs();

            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                IssuerSigningKeys = jwks,
                TryAllIssuerSigningKeys = true,
            }, out SecurityToken validatedToken);

            return (JwtSecurityToken)validatedToken;
        }
    }
}