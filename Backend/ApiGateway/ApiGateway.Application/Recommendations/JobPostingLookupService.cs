using ApiGateway.Application.Abstractions;
using ApiGateway.Application.Models;

namespace ApiGateway.Application.Recommendations;

public sealed class JobPostingLookupService(
    IJobRecommendationRepository recommendations,
    IFeedCoreGateway feedCoreGateway)
{
    public async Task<JobPostingDto?> GetRecommendedAsync(
        Guid userId,
        Guid jobPostingId,
        CancellationToken cancellationToken)
    {
        if (!await recommendations.ExistsAsync(userId, jobPostingId, cancellationToken))
            return null;

        return await feedCoreGateway.GetJobPostingAsync(jobPostingId, cancellationToken);
    }
}
