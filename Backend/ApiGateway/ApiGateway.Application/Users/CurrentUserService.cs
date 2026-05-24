using ApiGateway.Application.Abstractions;
using ApiGateway.Core;
using ApiGateway.Core.Entities;

namespace ApiGateway.Application.Users;

public sealed class CurrentUserService(IUserRepository users, IUnitOfWork unitOfWork)
{
    public async Task<User> GetOrCreateAsync(
        AuthenticatedUser authenticatedUser,
        CancellationToken cancellationToken = default)
    {
        var firstName = NormalizeProfileValue(authenticatedUser.FirstName);
        var lastName = NormalizeProfileValue(authenticatedUser.LastName);

        var user = await users.GetByKeycloakSubjectAsync(authenticatedUser.Subject, cancellationToken);
        if (user is null)
        {
            user = new User
            {
                KeycloakSubject = authenticatedUser.Subject,
                Email = authenticatedUser.Email,
                FirstName = firstName,
                LastName = lastName,
            };

            users.Add(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return user;
        }

        if (user.Email != authenticatedUser.Email || user.FirstName != firstName || user.LastName != lastName)
        {
            user.Email = authenticatedUser.Email;
            user.FirstName = firstName;
            user.LastName = lastName;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return user;
    }

    private static string? NormalizeProfileValue(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
