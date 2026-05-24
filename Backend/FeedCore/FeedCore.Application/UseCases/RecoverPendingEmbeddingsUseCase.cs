using FeedCore.Application.Abstractions;

namespace FeedCore.Application.UseCases;

public sealed class RecoverPendingEmbeddingsUseCase(
    IFeedCoreStore store,
    TimeProvider timeProvider)
{
    public Task<int> ExecuteAsync(CancellationToken cancellationToken)
        => store.RecoverProcessingJobPostingsAsync(timeProvider.GetUtcNow(), cancellationToken);
}
