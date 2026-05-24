using ApiGateway.Application.Abstractions;
using ApiGateway.Core;
using ApiGateway.Core.Entities;

namespace ApiGateway.Application.Recommendations;

public sealed class LiveRecommendationService(
    IUserRepository users,
    IJobRecommendationRepository recommendations,
    IUnitOfWork unitOfWork)
{
    public async Task<StoredLiveRecommendations> StoreAsync(
        StoreLiveRecommendationCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var userIds = command.UserIds.Distinct().ToList();
        var existingUsers = await users.ListExistingIdsAsync(userIds, cancellationToken);
        var existingRecommendations = await recommendations.ListExistingRecommendedUserIdsAsync(
            command.JobPostingId,
            existingUsers,
            cancellationToken);

        var insertedUserIds = new List<Guid>();
        foreach (var userId in existingUsers.Except(existingRecommendations))
        {
            recommendations.Add(new JobRecommendation
            {
                UserId = userId,
                JobPostingId = command.JobPostingId,
                RecommendedAt = now,
                Source = RecommendationSource.Live
            });
            insertedUserIds.Add(userId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new StoredLiveRecommendations(command.JobPostingId, insertedUserIds, now);
    }
}
