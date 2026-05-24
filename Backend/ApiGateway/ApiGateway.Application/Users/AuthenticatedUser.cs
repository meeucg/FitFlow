namespace ApiGateway.Application.Users;

public sealed record AuthenticatedUser(
    string Subject,
    string Email,
    string? FirstName,
    string? LastName);
