using System.Threading;
using System.Threading.Tasks;

namespace Mesawer.ApplicationLayer.CustomIdentity.Interfaces
{
    public interface IUserValidatorService
    {
        Task<bool> VerifyUserIdentityAsync(string userId, string token, CancellationToken ct);
        Task ValidateUserIdentityAsync(string userId, string token, CancellationToken ct);
        Task<(string token, string refreshToken)> UpdateUserSessionAsync(string userId, CancellationToken ct);
        Task ResetUserSessionAsync(string userId, bool forced, CancellationToken ct);
    }
}
