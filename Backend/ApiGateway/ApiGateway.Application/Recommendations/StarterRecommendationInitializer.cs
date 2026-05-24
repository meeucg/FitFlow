using ApiGateway.Application.Abstractions;
using ApiGateway.Application.Exceptions;
using ApiGateway.Core;
using ApiGateway.Core.Entities;

namespace ApiGateway.Application.Recommendations;

public sealed class StarterRecommendationInitializer(
    IUserRepository users,
    IJobRecommendationRepository recommendations,
    IInterviewGateway interviewGateway,
    IFeedCoreGateway feedCoreGateway,
    IRecommendationNotifier notifier,
    IUnitOfWork unitOfWork)
{
    private const int MaxSanitizedErrorLength = 1000;

    public async Task<IReadOnlyList<StarterRecommendationInitializationResult>> ProcessBatchAsync(
        StarterRecommendationInitializationSettings settings,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var retryBefore = now - settings.RetryDelay;
        var pendingUsers = await users.ListForRecommendationInitializationAsync(
            retryBefore,
            settings.MaxRetries,
            settings.BatchSize,
            cancellationToken);

        var results = new List<StarterRecommendationInitializationResult>();
        foreach (var user in pendingUsers)
            results.Add(await InitializeUserAsync(user, cancellationToken));

        return results;
    }

    private async Task<StarterRecommendationInitializationResult> InitializeUserAsync(
        User user,
        CancellationToken cancellationToken)
    {
        try
        {
            if (user.CurrentInterviewId is null)
                throw new InvalidOperationException("User has no current interview id.");

            var conclusion = await interviewGateway.GetInterviewConclusionAsync(
                user.CurrentInterviewId.Value,
                cancellationToken);

            if (conclusion is null)
                throw new InvalidOperationException("Interview conclusion is not available yet.");

            var recommendationIds = await feedCoreGateway.AddNewUserAsync(user.Id, conclusion, cancellationToken);
            var distinctRecommendationIds = recommendationIds.Distinct().ToList();

            var now = DateTimeOffset.UtcNow;
            var existingIds = distinctRecommendationIds.Count > 0
                ? await recommendations.ListExistingJobPostingIdsAsync(user.Id, distinctRecommendationIds, cancellationToken)
                : [];

            foreach (var recommendationId in distinctRecommendationIds.Except(existingIds))
            {
                recommendations.Add(new JobRecommendation
                {
                    UserId = user.Id,
                    JobPostingId = recommendationId,
                    RecommendedAt = now,
                    Source = RecommendationSource.Starter
                });
            }

            user.RecommendationState = RecommendationInitializationState.Ready;
            user.RecommendationInitializedAt = now;
            user.RecommendationLastError = null;
            user.UpdatedAt = now;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            notifier.PublishBatch(user.Id, distinctRecommendationIds, now);

            return StarterRecommendationInitializationResult.Success(user.Id, distinctRecommendationIds.Count);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            var error = exception is ExternalGatewayException gatewayException
                ? gatewayException.Message
                : Sanitize(exception);

            await MarkFailedAsync(user, error, cancellationToken);
            return StarterRecommendationInitializationResult.Failed(user.Id, error);
        }
    }

    private async Task MarkFailedAsync(
        User user,
        string error,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        user.RecommendationState = RecommendationInitializationState.Failed;
        user.RecommendationRetryCount++;
        user.RecommendationRequestedAt = now;
        user.RecommendationLastError = Sanitize(error);
        user.UpdatedAt = now;
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string Sanitize(Exception exception)
        => Sanitize(exception.GetBaseException().Message);

    private static string Sanitize(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            message = "Recommendation initialization failed.";

        return message.Length <= MaxSanitizedErrorLength
            ? message
            : message[..MaxSanitizedErrorLength];
    }
}

public sealed record StarterRecommendationInitializationSettings(
    TimeSpan RetryDelay,
    int MaxRetries,
    int BatchSize);

public sealed record StarterRecommendationInitializationResult(
    Guid UserId,
    bool Succeeded,
    int RecommendationCount,
    string? Error)
{
    public static StarterRecommendationInitializationResult Success(Guid userId, int recommendationCount)
        => new(userId, true, recommendationCount, null);

    public static StarterRecommendationInitializationResult Failed(Guid userId, string error)
        => new(userId, false, 0, error);
}
