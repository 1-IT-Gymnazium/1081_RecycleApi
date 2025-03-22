using System.Security.Claims;

namespace Recycle.Api.Utilities;

/// <summary>
/// Extension methods for extracting user information from <see cref="ClaimsPrincipal"/>.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Returns the username from the authenticated user's claims.
    /// </summary>
    public static string GetName(this ClaimsPrincipal user)
    {
        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            throw new InvalidOperationException("user not logged in");
        }
        var name = user.Claims.First(x => x.Type == ClaimTypes.Name).Value;
        return name;
    }

    /// <summary>
    /// Returns the user ID (as <see cref="Guid"/>) from the authenticated user's claims.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            throw new InvalidOperationException("user not logged in");
        }
        var idString = user.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        return Guid.Parse(idString);
    }
}
