using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Mesawer.ApplicationLayer.Extensions;

public static class ClaimsPrincipleExtensions
{
    /// <summary>
    /// Get the <paramref name="claimType"/> claim.
    /// </summary>
    public static Claim Claim(this ClaimsPrincipal principal, string claimType) => principal?.FindFirst(claimType);

    /// <summary>
    /// Get all claims of type <paramref name="claimType"/>
    /// </summary>
    public static IList<Claim> Claims(this ClaimsPrincipal claimsPrincipal, string claimType)
        => claimsPrincipal?.FindAll(claimType).ToList();

    /// <summary>
    /// Get user roles collection.
    /// </summary>
    public static IList<string> Roles(this ClaimsPrincipal principal, string claimType = ClaimTypes.Role)
        => principal?.Claims.Where(c => c.Type == claimType).Select(c => c.Value).ToList();

    /// <summary>
    /// Get user roles as Enum collection of type <typeparamref name="T"/>.
    /// </summary>
    public static List<T> Roles<T>(this ClaimsPrincipal principal, string claimType = ClaimTypes.Role)
        where T : struct
        => principal?.Claims.Where(c => c.Type == claimType)
            .Select(c => c.Value.ToEnum<T>()).ToList();
}
