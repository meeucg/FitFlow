using System.Security.Claims;
using ApiGateway.Application.Users;

namespace ApiGateway.Authentication;

internal static class AuthenticatedUserClaims
{
    public static AuthenticatedUser From(ClaimsPrincipal principal)
    {
        var subject = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? principal.FindFirstValue("sub")
                      ?? throw new InvalidOperationException("Authenticated user token is missing the 'sub' claim.");

        var email = principal.FindFirstValue(ClaimTypes.Email)
                    ?? principal.FindFirstValue("email")
                    ?? $"{subject}@unknown.local";

        return new AuthenticatedUser(
            subject,
            email,
            principal.FindFirstValue(ClaimTypes.GivenName) ?? principal.FindFirstValue("given_name"),
            principal.FindFirstValue(ClaimTypes.Surname) ?? principal.FindFirstValue("family_name"));
    }
}
