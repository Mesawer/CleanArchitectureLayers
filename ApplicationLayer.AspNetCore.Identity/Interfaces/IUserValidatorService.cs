using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Mesawer.ApplicationLayer.Exceptions;

namespace Mesawer.ApplicationLayer.AspNetCore.Identity.Interfaces;

public interface IUserValidatorService
{
    /// <summary>
    /// Validates user identity using a certain session <paramref name="token"/>
    /// </summary>
    /// <param name="userId">User's Id</param>
    /// <param name="token">Session token, or null to create/reset session</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>A boolean indicating whether valid or not</returns>
    /// <remarks>Thread Safe</remarks>
    Task<bool> VerifyUserIdentityAsync([NotNull] string userId, [NotNull] string token, CancellationToken ct);

    /// <summary>
    /// Validates user identity using a certain session <paramref name="token"/>
    /// </summary>
    /// <param name="userId">User's Id</param>
    /// <param name="token">Session token, or null to create/reset session</param>
    /// <param name="ct">Cancellation Token</param>
    /// <exception cref="ForbiddenAccessException"></exception>
    /// <exception cref="BadRequestException"></exception>
    Task ValidateUserIdentityAsync([NotNull] string userId, [NotNull] string token, CancellationToken ct);

    /// <summary>
    /// Regenerates session
    /// </summary>
    Task<(string token, string refreshToken)> UpdateUserSessionAsync([NotNull] string userId, CancellationToken ct);

    /// <summary>
    /// Resets User Session if exists
    /// </summary>
    /// <param name="userId">User's Id</param>
    /// <param name="forced">true to reset the MacAddress</param>
    /// <param name="ct">Cancellation Token</param>
    Task ResetUserSessionAsync([NotNull] string userId, bool forced, CancellationToken ct);
}
