using System.Threading.Tasks;
using Mesawer.ApplicationLayer.Models;

namespace Mesawer.ApplicationLayer.Interfaces
{
    public interface ITokenService<in T> where T : ApplicationUser
    {
        /// <summary>
        /// Generates jwt token for an authenticated user.
        /// </summary>
        Task<AuthResponse> GenerateAuthJwtToken(T user);

        /// <summary>
        /// Generates jwt token for an external user.
        /// </summary>
        Task<(SocialUser user, AuthResponse response)> GenerateAuthJwtToken(string provider, string token);

        /// <summary>
        /// Validates user's external login.
        /// </summary>
        Task<SocialUser> ValidateExternalLogin(string provider, string token);
    }
}
