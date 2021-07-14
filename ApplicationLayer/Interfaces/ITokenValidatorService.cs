using Mesawer.ApplicationLayer.Enums;
using Mesawer.ApplicationLayer.Models;

namespace Mesawer.ApplicationLayer.Interfaces
{
    public interface ITokenValidatorService
    {
        bool Verify(TokenType type, string userId, string token);
        TokenObject Validate(TokenType type, string userId, string token);
    }
}
