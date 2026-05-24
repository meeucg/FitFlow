using ApiGateway.Core;
using ApiGateway.Core.Entities;

namespace ApiGateway.Application.Abstractions;

public interface IJobRecommendationRepository
{
    void Add(JobRecommendation recommendation);

    Task<bool> ExistsAsync(Guid userId, Guid jobPostingId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> ListExistingJobPostingIdsAsync(
        Guid userId,
        IReadOnlyCollection<Guid> jobPostingIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> ListExistingRecommendedUserIdsAsync(
        Guid jobPostingId,
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<JobRecommendation>> ListStarterAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<JobRecommendation>> ListAllAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<JobRecommendation>> ListAfterAsync(
        Guid userId,
        DateTimeOffset cursor,
        CancellationToken cancellationToken);
}
