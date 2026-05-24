using ApiGateway.Core;
using ApiGateway.Core.Entities;

namespace ApiGateway.Application.Abstractions;

public interface IUserRepository
{
    void Add(User user);

    Task<User?> GetByKeycloakSubjectAsync(string keycloakSubject, CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> ListExistingIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<User>> ListForRecommendationInitializationAsync(
        DateTimeOffset retryBefore,
        int maxRetries,
        int take,
        CancellationToken cancellationToken);
}
