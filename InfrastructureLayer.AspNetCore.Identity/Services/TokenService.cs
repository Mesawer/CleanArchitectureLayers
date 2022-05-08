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
using Mesawer.ApplicationLayer.AspNetCore.Identity.Interfaces;
using Mesawer.ApplicationLayer.AspNetCore.Identity.Models;
using Mesawer.InfrastructureLayer.Resources;
using Mesawer.ApplicationLayer.Exceptions;
using Mesawer.ApplicationLayer.Models;
using Mesawer.DomainLayer.AspNetCore.Identity.Entities;
using Mesawer.DomainLayer.AspNetCore.Identity.Enums;
using Mesawer.DomainLayer.Entities;
using Mesawer.InfrastructureLayer.AspNetCore.Identity.Models;
using Mesawer.InfrastructureLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Mesawer.InfrastructureLayer.AspNetCore.Identity.Services;

public class TokenService<TUser, TAccount, TSession> : ITokenService<TUser>
    where TUser : ApplicationUser where TAccount : Account<TUser> where TSession : Session
{
    private const string GoogleTokenInfoEndpoint =
        "https://oauth2.googleapis.com/tokeninfo?id_token={0}";

    private const string FacebookTokenInfoEndpoint =
        "https://graph.facebook.com/v4.0/me?access_token={0}&fields=email,first_name,last_name,picture";

    private const string GoogleLoginProvider   = "google";
    private const string FacebookLoginProvider = "facebook";
    private const string AppleLoginProvider    = "apple";

    private readonly IIdentityManager<TUser> _identityManager;
    private readonly AppleJwtVerifier        _appleJwtVerifier;
    private readonly IHostEnvironment        _env;
    private readonly JwtConfig               _jwtConfig;

    public TokenService(
        IIdentityManager<TUser> identityManager,
        AppleJwtVerifier appleJwtVerifier,
        IOptionsMonitor<JwtConfig> config,
        IHostEnvironment env)
    {
        _identityManager  = identityManager;
        _appleJwtVerifier = appleJwtVerifier;
        _env              = env;
        _jwtConfig        = config.CurrentValue;
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
            AppleLoginProvider    => LoginProvider.Apple,
            _                     => throw new BadRequestException(SharedRes.NotSupportedProvider)
        };

        var socialUser = loginProvider switch
        {
            LoginProvider.Facebook => await ValidateFacebookAccessToken(token),
            LoginProvider.Google   => await ValidateGoogleIdToken(token),
            LoginProvider.Apple    => await ValidateAppleIdentityToken(token),
            LoginProvider.Twitter  => throw new NotImplementedException(),
            _                      => throw new ArgumentOutOfRangeException()
        };

        var user = await _identityManager.UserManager.FindByEmailAsync(socialUser.Email);

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
                await _identityManager.UserManager.AddLoginAsync(user,
                    new UserLoginInfo(provider, socialUser.Id, provider));

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
            AppleLoginProvider    => await ValidateAppleIdentityToken(token),
            _                     => throw new BadRequestException(SharedRes.NotSupportedProvider)
        };

    // Validates Google IdToken as in https://developers.google.com/identity/sign-in/web/backend-auth#calling-the-tokeninfo-endpoint
    private async Task<SocialUser> ValidateGoogleIdToken(string tokenId)
    {
        var result = await SendRequest(GoogleTokenInfoEndpoint, tokenId);

        var response = JsonConvert.DeserializeObject<GoogleResponse>(result);

        if (response?.Id is null || response.Email is null) throw new BadRequestException(SharedRes.InvalidLogin);

        return new SocialUser
        {
            Provider = LoginProvider.Google,
            Id       = response.Id,
            Email    = response.Email,
            FullName = response.FirstName is not null || response.LastName is not null
                ? FullNameDto.Create(response.FirstName, response.LastName)
                : FullNameDto.Create("Google", "User"),
            Picture = response.Picture
        };
    }

    // Validates Facebook AccessToken
    // Note: Facebook documentation sucks -_-
    private async Task<SocialUser> ValidateFacebookAccessToken(string token)
    {
        var result = await SendRequest(FacebookTokenInfoEndpoint, token);

        var response = JsonConvert.DeserializeObject<FacebookResponse>(result);

        if (response?.Id is null || response.Email is null) throw new BadRequestException(SharedRes.InvalidLogin);

        return new SocialUser
        {
            Provider = LoginProvider.Facebook,
            Id       = response.Id,
            Email    = response.Email,
            FullName = response.FirstName is not null || response.LastName is not null
                ? FullNameDto.Create(response.FirstName, response.LastName)
                : FullNameDto.Create("Facebook", "User"),
            Picture = response.Picture.Data.Url
        };
    }

    // Validates Apple IdentityToken
    // As in https://developer.apple.com/documentation/sign_in_with_apple/sign_in_with_apple_rest_api/verifying_a_user
    private async Task<SocialUser> ValidateAppleIdentityToken(string token)
    {
        var response = await _appleJwtVerifier.Verify(token);

        if (response?.Id is null || response.Email is null) throw new BadRequestException(SharedRes.InvalidLogin);

        return new SocialUser
        {
            Provider = LoginProvider.Apple,
            Id       = response.Id,
            Email    = response.Email,
            FullName = response.Name is not null
                ? FullNameDto.Create(response.Name.FirstName, response.Name.LastName)
                : FullNameDto.Create("Apple", "User")
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
        claims.Add(new Claim(AppClaims.UserName, user.UserName));
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
