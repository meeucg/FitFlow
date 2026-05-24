using ApiGateway.Application.Abstractions;
using ApiGateway.Core;
using ApiGateway.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiGateway.Persistence;

internal sealed class UserRepository(ApiGatewayDbContext dbContext) : IUserRepository
{
    public void Add(User user)
    {
        dbContext.Users.Add(user);
    }

    public Task<User?> GetByKeycloakSubjectAsync(string keycloakSubject, CancellationToken cancellationToken)
        => dbContext.Users.SingleOrDefaultAsync(
            x => x.KeycloakSubject == keycloakSubject,
            cancellationToken);

    public async Task<IReadOnlyList<Guid>> ListExistingIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .Where(x => ids.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> ListForRecommendationInitializationAsync(
        DateTimeOffset retryBefore,
        int maxRetries,
        int take,
        CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .Where(user =>
                user.RecommendationState == RecommendationInitializationState.Pending ||
                (user.RecommendationState == RecommendationInitializationState.Failed &&
                 user.RecommendationRetryCount < maxRetries &&
                 user.RecommendationRequestedAt <= retryBefore))
            .OrderBy(user => user.RecommendationRequestedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
