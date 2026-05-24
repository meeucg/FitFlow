using FeedCore.Application.Models;
using FeedCore.Core.Models;

namespace FeedCore.Application.Abstractions;

public interface IFeedCoreStore
{
    Task<IReadOnlyList<Guid>> UpsertUserEmbeddingAndFindStarterRecommendationsAsync(
        Guid userId,
        EmbeddingVector embedding,
        DateTimeOffset postedAfter,
        double maxCosineDistance,
        int limit,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task<AcceptNormalizedJobPostingResult> SaveNormalizedPostingAsync(
        NormalizedJobPostingInput posting,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PendingJobPosting>> ClaimPendingJobPostingsAsync(
        int limit,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> FindMatchingUserIdsAsync(
        EmbeddingVector embedding,
        double maxCosineDistance,
        int limit,
        CancellationToken cancellationToken);

    Task CompleteJobEmbeddingAsync(
        Guid jobPostingId,
        EmbeddingVector embedding,
        IReadOnlyList<Guid> matchedUserIds,
        string recommendationsExchange,
        string recommendationsRoutingKey,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task MarkJobEmbeddingFailedAsync(
        Guid jobPostingId,
        string sanitizedError,
        DateTimeOffset nextAttemptAt,
        CancellationToken cancellationToken);

    Task<int> RecoverProcessingJobPostingsAsync(DateTimeOffset now, CancellationToken cancellationToken);

    Task<JobPostingDisplayData?> GetJobPostingDisplayAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<OutboxMessage>> ClaimPendingOutboxMessagesAsync(
        int limit,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task MarkOutboxPublishedAsync(Guid id, DateTimeOffset now, CancellationToken cancellationToken);

    Task MarkOutboxFailedAsync(
        Guid id,
        string sanitizedError,
        DateTimeOffset nextAttemptAt,
        CancellationToken cancellationToken);

    Task<int> RecoverProcessingOutboxMessagesAsync(DateTimeOffset now, CancellationToken cancellationToken);
}
