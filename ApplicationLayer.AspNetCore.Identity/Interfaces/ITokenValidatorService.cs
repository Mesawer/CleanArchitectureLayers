using Mesawer.ApplicationLayer.Enums;
using Mesawer.ApplicationLayer.Models;

namespace Mesawer.ApplicationLayer.AspNetCore.Identity.Interfaces
{
    public interface ITokenValidatorService
    {
        bool Verify(TokenType type, string userId, string token);
        TokenObject Validate(TokenType type, string userId, string token);
    }
}
