using System.Security.Claims;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

public static class ClaimsPrincipalExtensions
{
    public static int GetRequiredUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("The authenticated user ID is missing.");
    }

    public static bool IsPrivileged(this ClaimsPrincipal user) =>
        user.IsInRole("Admin") || user.IsInRole("Manager");
}
