using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Mesawer.InfrastructureLayer.AspNetCore.Identity.Models
{
    public static class AppleJwtVerifier
    {
        public static async Task<AppleInfo> Verify(string token)
        {
            const string issuer   = "https://appleid.apple.com";
            const string clientId = "com.QMESolutions.QME";

            var validSignature = await VerifySignature(token);

            if (!validSignature) return null;

            var data = GetData(token);

            var validNonce = data.NonceSupported;

            if (!validNonce) return null;

            var validIssuer = data.Issuer == issuer;

            if (!validIssuer) return null;

            var validAudience = data.Audience == clientId;

            if (!validAudience) return null;

            var date = DateTime.UnixEpoch.AddMilliseconds(data.Expiration * 1000);

            return date >= DateTime.UtcNow ? data : null;
        }

        private static async Task<bool> VerifySignature(string token)
        {
            var client   = new HttpClient();
            var response = await client.GetAsync("https://appleid.apple.com/auth/keys");

            if (!response.IsSuccessStatusCode) return false;

            var content = await response.Content.ReadAsStringAsync();

            var keys = JsonConvert.DeserializeObject<AppleKey>(content)?.Keys;

            if (keys is null) return false;

            return keys
                .Select(key => VerifySignature(key.Modulus, key.Exponent, token))
                .Any(value => value);
        }

        private static bool VerifySignature(string modulus, string exponent, string token)
        {
            var tokenParts = token.Split('.');

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(
                new RSAParameters
                {
                    Modulus  = FromBase64Url(modulus),
                    Exponent = FromBase64Url(exponent)
                });

            var sha256 = SHA256.Create();
            var hash   = sha256.ComputeHash(Encoding.UTF8.GetBytes(tokenParts[0] + '.' + tokenParts[1]));

            var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            rsaDeformatter.SetHashAlgorithm("SHA256");

            return rsaDeformatter.VerifySignature(hash, FromBase64Url(tokenParts[2]));

            static byte[] FromBase64Url(string base64Url)
            {
                var padded = base64Url.Length % 4 == 0
                    ? base64Url
                    : base64Url + "===="[(base64Url.Length % 4)..];
                var base64 = padded.Replace("_", "/")
                    .Replace("-", "+");
                return Convert.FromBase64String(base64);
            }
        }

        private static AppleInfo GetData(string token)
        {
            var handler   = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token);
            var claims    = jsonToken is JwtSecurityToken tokenS ? tokenS.Claims.ToList() : null;

            if (claims is null) return null;

            var properties = typeof(AppleInfo)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToList();

            var info = new AppleInfo
            {
                Id             = FindClaim(nameof(AppleInfo.Id)),
                Issuer         = FindClaim(nameof(AppleInfo.Issuer)),
                Audience       = FindClaim(nameof(AppleInfo.Audience)),
                Expiration     = Convert.ToInt64(FindClaim(nameof(AppleInfo.Expiration))),
                Email          = FindClaim(nameof(AppleInfo.Email)),
                EmailVerified  = Convert.ToBoolean(FindClaim(nameof(AppleInfo.EmailVerified))),
                Name           = JsonConvert.DeserializeObject<AppleInfo.FullName>(FindClaim(nameof(AppleInfo.Name))),
                NonceSupported = Convert.ToBoolean(FindClaim(nameof(AppleInfo.NonceSupported))),
                Data           = claims.ToDictionary(c => c.Type, c => c.Value)
            };

            return info;

            string FindClaim(string key)
            {
                var jsonName = properties.FirstOrDefault(c => c.Name == key)
                    ?.GetCustomAttribute<JsonPropertyAttribute>()
                    ?.PropertyName;

                return jsonName is not null ? claims.FirstOrDefault(c => c.Type == jsonName)?.Value : null;
            }
        }
    }

    public class AppleKey
    {
        public List<Key> Keys { get; set; }

        public class Key
        {
            [JsonProperty("n")]
            public string Modulus { get; set; }

            [JsonProperty("e")]
            public string Exponent { get; set; }
        }
    }

    public class AppleInfo
    {
        [JsonProperty("sub")]
        public string Id { get; set; }

        [JsonProperty("iss")]
        public string Issuer { get; set; }

        [JsonProperty("aud")]
        public string Audience { get; set; }

        [JsonProperty("exp")]
        public long Expiration { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("email_verified")]
        public bool EmailVerified { get; set; }

        [JsonProperty("name")]
        public FullName Name { get; set; }

        [JsonProperty("nonce_supported")]
        public bool NonceSupported { get; set; }

        public Dictionary<string, string> Data { get; set; }

        public class FullName
        {
            [JsonProperty("firstName")]
            public string FirstName { get; set; }

            [JsonProperty("lastName")]
            public string LastName { get; set; }
        }
    }
}
