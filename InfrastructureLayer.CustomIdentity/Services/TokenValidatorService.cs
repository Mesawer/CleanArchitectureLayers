using Mesawer.ApplicationLayer.CustomIdentity.Interfaces;
using Mesawer.ApplicationLayer.Enums;
using Mesawer.ApplicationLayer.Extensions;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.ApplicationLayer.Models;

namespace Mesawer.InfrastructureLayer.CustomIdentity.Services
{
    public class TokenValidatorService : ITokenValidatorService
    {
        private readonly IMemoryCacheService _cache;
        private readonly IDateTime           _dateTime;

        public TokenValidatorService(IMemoryCacheService cache, IDateTime dateTime)
        {
            _cache    = cache;
            _dateTime = dateTime;
        }

        public bool Verify(TokenType type, string userId, string token) => Get(type, userId, token) is not null;

        public TokenObject Validate(TokenType type, string userId, string token)
        {
            var tokenObj = Get(type, userId, token);

            if (tokenObj is null) return null;

            _cache.Remove(type, userId);

            return tokenObj;
        }

        private TokenObject Get(TokenType type, string userId, string token)
        {
            var tokenObj = _cache.Get(type, userId);

            if (tokenObj is null) return null;

            if (tokenObj.ExpiresAt.IsOlderThan(_dateTime.Now) || tokenObj.NumberOfTries == 5)
            {
                _cache.Remove(type, userId);

                return null;
            }

            if (tokenObj.Token == token) return tokenObj;

            tokenObj.NumberOfTries++;

            _cache.Add(userId, tokenObj);

            return null;
        }
    }
}
