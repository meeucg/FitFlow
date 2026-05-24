using ApiGateway.Application.Abstractions;
using ApiGateway.Core;
using ApiGateway.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiGateway.Persistence;

internal sealed class JobRecommendationRepository(ApiGatewayDbContext dbContext) : IJobRecommendationRepository
{
    public void Add(JobRecommendation recommendation)
    {
        dbContext.JobRecommendations.Add(recommendation);
    }

    public Task<bool> ExistsAsync(Guid userId, Guid jobPostingId, CancellationToken cancellationToken)
        => dbContext.JobRecommendations.AnyAsync(
            x => x.UserId == userId && x.JobPostingId == jobPostingId,
            cancellationToken);

    public async Task<IReadOnlyList<Guid>> ListExistingJobPostingIdsAsync(
        Guid userId,
        IReadOnlyCollection<Guid> jobPostingIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.JobRecommendations
            .Where(x => x.UserId == userId && jobPostingIds.Contains(x.JobPostingId))
            .Select(x => x.JobPostingId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> ListExistingRecommendedUserIdsAsync(
        Guid jobPostingId,
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.JobRecommendations
            .Where(x => x.JobPostingId == jobPostingId && userIds.Contains(x.UserId))
            .Select(x => x.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobRecommendation>> ListStarterAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.JobRecommendations
            .Where(x => x.UserId == userId && x.Source == RecommendationSource.Starter)
            .OrderBy(x => x.RecommendedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobRecommendation>> ListAllAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.JobRecommendations
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.RecommendedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobRecommendation>> ListAfterAsync(
        Guid userId,
        DateTimeOffset cursor,
        CancellationToken cancellationToken)
    {
        return await dbContext.JobRecommendations
            .Where(x => x.UserId == userId && x.RecommendedAt > cursor)
            .OrderBy(x => x.RecommendedAt)
            .ToListAsync(cancellationToken);
    }
}
