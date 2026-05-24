namespace ApiGateway.Application.Models;

/// <summary>
/// Public service status response returned by the root endpoint.
/// </summary>
/// <param name="Service">Name of the running service.</param>
public sealed record ServiceStatusDto(string Service);

/// <summary>
/// Current authenticated user profile stored by ApiGateway.
/// </summary>
public sealed record CurrentUserDto
{
    /// <summary>
    /// ApiGateway local user identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Email address synchronized from the Keycloak access token.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Optional first name synchronized from the Keycloak access token.
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Optional last name synchronized from the Keycloak access token.
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// Indicates whether this user already has a server-owned interview id.
    /// </summary>
    public bool HasInterview { get; init; }
}
