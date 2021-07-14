using System;
using System.Linq;
using System.Threading.Tasks;
using Mesawer.ApplicationLayer.Enums;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.ApplicationLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using IdentityOptions = Mesawer.InfrastructureLayer.Models.IdentityOptions;

namespace Mesawer.InfrastructureLayer.Identity
{
    public class TokenGeneratorService<T> : ITokenGeneratorService<T> where T : ApplicationUser
    {
        private readonly UserManager<T>         _userManager;
        private readonly IMemoryCacheService    _cache;
        private readonly IDateTime              _dateTime;
        private readonly IdentityOptions _identityOptions;

        public TokenGeneratorService(
            UserManager<T> userManager,
            IMemoryCacheService cache,
            IDateTime dateTime,
            IOptions<IdentityOptions> userIdentityOptions)
        {
            _userManager     = userManager;
            _cache           = cache;
            _dateTime        = dateTime;
            _identityOptions = userIdentityOptions.Value;
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(T user)
        {
            var longToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            if (!_identityOptions.NumericVerificationToken) return longToken;

            var key = GenerateToken();

            AddToCache(TokenType.ConfirmEmail, user.Id, key, longToken);

            return key;
        }

        public async Task<string> GenerateChangeEmailTokenAsync(T user, string newEmail)
        {
            var longToken = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);

            if (!_identityOptions.NumericVerificationToken) return longToken;

            var key = GenerateToken();

            AddToCache(TokenType.ChangeEmail, user.Id, key, longToken, newEmail);

            return key;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(T user)
        {
            var longToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            if (!_identityOptions.NumericVerificationToken) return longToken;

            var key = GenerateToken();

            AddToCache(TokenType.ResetPassword, user.Id, key, longToken);

            return key;
        }

        public Task<string> GeneratePhoneNumberConfirmationTokenAsync(T user)
        {
            var key = GenerateToken();

            AddToCache(TokenType.ConfirmPhoneNumber, user.Id, key, null);

            return Task.FromResult(key);
        }

        public async Task<string> GenerateChangePhoneNumberTokenAsync(T user, string phoneNumber)
        {
            var longToken = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
            var key       = GenerateToken();

            AddToCache(TokenType.ChangePhoneNumber, user.Id, key, longToken, phoneNumber);

            return key;
        }

        private void AddToCache(TokenType type, string userId, string token, string longToken, object extraData = null)
            => _cache.Add(userId,
                new TokenObject
                {
                    UserId    = userId,
                    Type      = type,
                    Token     = token,
                    LongToken = longToken,
                    ExpiresAt = _dateTime.Now.AddMinutes(_identityOptions.TokenExpirationPeriod),
                    ExtraData = extraData
                });

        private static string GenerateToken()
            => Guid.NewGuid().ToString("N").ToUpper()[..3]
                .ToCharArray()
                .Aggregate(string.Empty, (current, next) => current + (int) next);
    }
}
