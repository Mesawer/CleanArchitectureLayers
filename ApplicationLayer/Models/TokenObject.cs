using System;
using Mesawer.ApplicationLayer.Enums;

namespace Mesawer.ApplicationLayer.Models
{
    public class TokenObject
    {
        public string UserId { get; set; }

        public TokenType Type { get; set; }

        public string Token { get; set; }

        public string LongToken { get; set; }

        public int NumberOfTries { get; set; }

        public DateTimeOffset ExpiresAt { get; set; }

        public object ExtraData { get; set; }
    }
}
