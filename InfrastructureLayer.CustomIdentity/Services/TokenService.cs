using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Mesawer.ApplicationLayer.CustomIdentity.Interfaces;
using Mesawer.ApplicationLayer.CustomIdentity.Models;
using Mesawer.InfrastructureLayer.Resources;
using Mesawer.ApplicationLayer.Exceptions;
using Mesawer.ApplicationLayer.Models;
using Mesawer.DomainLayer.CustomIdentity.Entities;
using Mesawer.DomainLayer.CustomIdentity.Enums;
using Mesawer.InfrastructureLayer.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Mesawer.InfrastructureLayer.CustomIdentity.Services
{
    public class TokenService<TUser, TRole> : ITokenService<TUser> where TUser : ApplicationUser where TRole : Enum
    {
        private const string GoogleTokenInfoEndpoint =
            "https://oauth2.googleapis.com/tokeninfo?id_token={0}";

        private const string FacebookTokenInfoEndpoint =
            "https://graph.facebook.com/v4.0/me?access_token={0}&fields=email,first_name,last_name,picture";

        private const string GoogleLoginProvider   = "google";
        private const string FacebookLoginProvider = "facebook";

        private readonly IIdentityManager<TUser, TRole> _identityManager;
        private readonly IHostEnvironment               _env;
        private readonly JwtConfig                      _jwtConfig;

        public TokenService(
            IIdentityManager<TUser, TRole> identityManager,
            IOptionsMonitor<JwtConfig> config,
            IHostEnvironment env)
        {
            _identityManager = identityManager;
            _env             = env;
            _jwtConfig       = config.CurrentValue;
        }

        public async Task<AuthResponse> GenerateAuthJwtToken(TUser user)
        {
            var (token, refreshToken) =
                await _identityManager.UserValidator.UpdateUserSessionAsync(user.Id, default);

            var claims = await GetUserClaims(user, token);

            var expires = DateTime.UtcNow.AddDays(_jwtConfig.ExpirationPeriod);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer  = _jwtConfig.Issuer,
                Expires = expires,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key)),
                    SecurityAlgorithms.HmacSha512Signature),
            };

            var jwtToken = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

            return new AuthResponse
            {
                Token        = tokenHandler.WriteToken(jwtToken),
                RefreshToken = refreshToken
            };
        }

        /// <summary>
        /// Generate token using the login provider after validating that the login provider already register
        /// and the token was issued by the actual login provider
        /// </summary>
        public async Task<(SocialUser user, AuthResponse response)> GenerateAuthJwtToken(string provider, string token)
        {
            var loginProvider = provider.ToLower() switch
            {
                GoogleLoginProvider   => LoginProvider.Google,
                FacebookLoginProvider => LoginProvider.Facebook,
                _                     => throw new BadRequestException(SharedRes.NotSupportedProvider)
            };

            var socialUser = loginProvider switch
            {
                LoginProvider.Facebook => await ValidateFacebookAccessToken(token),
                LoginProvider.Google   => await ValidateGoogleIdToken(token),
                LoginProvider.Apple    => throw new NotImplementedException(),
                LoginProvider.Twitter  => throw new NotImplementedException(),
                _                      => throw new ArgumentOutOfRangeException()
            };

            var user = await _identityManager.UserFinder.FindByEmailAsync(socialUser.Email);

            if (user is null) return (null, null);

            var logins = (await _identityManager.UserManager.GetLoginsAsync(user)).ToList();

            //if (logins.All(c => !string.Equals(c.LoginProvider,
            //    loginProvider.ToString(),
            //    StringComparison.CurrentCultureIgnoreCase)))
            //    throw new BadRequestException(SharedRes.InvalidLogin);

            // Add the social user if it doesn't exist, since it belongs to this user
            if (logins.All(c => c.ProviderKey != socialUser.Id))
            {
                var result =
                    await _identityManager.UserManager.AddLoginAsync(user, socialUser);

                if (!result.Succeeded) throw new BadRequestException(SharedRes.InvalidLogin);
            }

            // If we reach this point, everything is fine
            return (socialUser, await GenerateAuthJwtToken(user));
        }

        /// <summary>
        /// Makes sure the token issuer was the actual login provider
        /// </summary>
        /// <returns>The login provider and the key to be stored in the database</returns>
        public async Task<SocialUser> ValidateExternalLogin(string provider, string token)
            => provider.ToLower() switch
            {
                GoogleLoginProvider   => await ValidateGoogleIdToken(token),
                FacebookLoginProvider => await ValidateFacebookAccessToken(token),
                _                     => throw new BadRequestException(SharedRes.NotSupportedProvider)
            };

        // Validates Google IdToken as in https://developers.google.com/identity/sign-in/web/backend-auth#calling-the-tokeninfo-endpoint
        private async Task<SocialUser> ValidateGoogleIdToken(string tokenId)
        {
            var result = await SendRequest(GoogleTokenInfoEndpoint, tokenId);

            var response = JsonConvert.DeserializeObject<GoogleResponse>(result);

            if (response is null) throw new BadRequestException(SharedRes.InvalidLogin);

            return new SocialUser
            {
                Provider = LoginProvider.Google,
                Id       = response.Id,
                Email    = response.Email,
                FullName = new FullNameDto
                {
                    FirstName = response.FirstName,
                    LastName  = response.LastName
                },
                Picture = response.Picture
            };
        }

        // Validates Facebook AccessToken
        // Note: Facebook documentation sucks -_-
        private async Task<SocialUser> ValidateFacebookAccessToken(string token)
        {
            var result = await SendRequest(FacebookTokenInfoEndpoint, token);

            var response = JsonConvert.DeserializeObject<FacebookResponse>(result);

            if (response is null) throw new BadRequestException(SharedRes.InvalidLogin);

            return new SocialUser
            {
                Provider = LoginProvider.Facebook,
                Id       = response.Id,
                Email    = response.Email,
                FullName = new FullNameDto
                {
                    FirstName = response.FirstName,
                    LastName  = response.LastName
                },
                Picture = response.Picture.Data.Url
            };
        }

        private async Task<IList<Claim>> GetUserClaims(TUser user, string token)
        {
            // Roles
            var claims = (await _identityManager.UserManager.GetRolesAsync(user))
                .Select(r => new Claim(AppClaims.Roles, r.ToString())).ToList();

            // Info
            claims.Add(new Claim(AppClaims.Created,
                user.CreatedAt.ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.DateTime));

            claims.Add(new Claim(AppClaims.Id, user.Id));
            claims.Add(new Claim(AppClaims.UserName, user.Username));
            claims.Add(new Claim(AppClaims.Email, user.Email));
            claims.Add(new Claim(AppClaims.Token, token));

            return claims;
        }

        private async Task<string> SendRequest(string url, string token)
        {
            var clientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (_, _, _, sslPolicyErrors) =>
                    {
                        if (_env.IsDevelopment() || _env.IsStaging()) return true;

                        return sslPolicyErrors == SslPolicyErrors.None;
                    },
            };

            var httpClient = new HttpClient(clientHandler)
            {
                BaseAddress = new Uri(string.Format(url, token)),
            };

            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var result = await httpClient.GetAsync(string.Empty);

            if (!result.IsSuccessStatusCode) throw new BadRequestException(SharedRes.FailedToConnect);

            return await result.Content.ReadAsStringAsync();
        }
    }
}
