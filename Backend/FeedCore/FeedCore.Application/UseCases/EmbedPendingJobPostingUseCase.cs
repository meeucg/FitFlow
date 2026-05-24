using FeedCore.Application.Abstractions;
using FeedCore.Application.Exceptions;
using FeedCore.Application.Models;
using FeedCore.Application.Options;
using FeedCore.Application.Rendering;
using Microsoft.Extensions.Options;

namespace FeedCore.Application.UseCases;

public sealed class EmbedPendingJobPostingUseCase(
    IFeedCoreStore store,
    IEmbeddingGenerator embeddingGenerator,
    JobPostingTextRenderer renderer,
    IOptions<RecommendationOptions> recommendationOptions,
    TimeProvider timeProvider)
{
    private const int MaxSanitizedErrorLength = 1000;

    public async Task<EmbedPendingJobPostingResult> ExecuteBatchAsync(
        string recommendationsExchange,
        string recommendationsRoutingKey,
        CancellationToken cancellationToken)
    {
        var options = recommendationOptions.Value;
        var now = timeProvider.GetUtcNow();
        var pending = await store.ClaimPendingJobPostingsAsync(
            options.PendingEmbeddingBatchSize,
            now,
            cancellationToken);

        var embedded = 0;
        var failed = 0;
        var recommendationsCreated = 0;

        foreach (var posting in pending)
        {
            try
            {
                var text = renderer.Render(posting.DisplayData);
                if (string.IsNullOrWhiteSpace(text))
                    throw new FeedCoreValidationException("Job posting rendered to empty embedding text.");

                var embedding = await embeddingGenerator.GenerateAsync(text, cancellationToken);
                EnsureDimensions(embedding, options.EmbeddingDimensions);

                var matchedUsers = await store.FindMatchingUserIdsAsync(
                    embedding,
                    options.MatchMaxCosineDistance,
                    options.MaxUsersPerJob,
                    cancellationToken);

                await store.CompleteJobEmbeddingAsync(
                    posting.Id,
                    embedding,
                    matchedUsers,
                    recommendationsExchange,
                    recommendationsRoutingKey,
                    timeProvider.GetUtcNow(),
                    cancellationToken);

                embedded++;
                if (matchedUsers.Count > 0)
                    recommendationsCreated++;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                failed++;
                await store.MarkJobEmbeddingFailedAsync(
                    posting.Id,
                    Sanitize(exception),
                    timeProvider.GetUtcNow() + Backoff(failed),
                    cancellationToken);
            }
        }

        return new EmbedPendingJobPostingResult(
            pending.Count,
            embedded,
            failed,
            recommendationsCreated);
    }

    private static TimeSpan Backoff(int failureNumber)
        => TimeSpan.FromSeconds(Math.Min(300, Math.Pow(2, Math.Min(failureNumber, 8))));

    private static void EnsureDimensions(EmbeddingVector embedding, int expectedDimensions)
    {
        if (embedding.Dimensions != expectedDimensions)
            throw new EmbeddingProviderException(
                $"Embedding provider returned {embedding.Dimensions} dimensions, expected {expectedDimensions}.");
    }

    private static string Sanitize(Exception exception)
    {
        var message = exception.GetBaseException().Message;
        if (string.IsNullOrWhiteSpace(message))
            message = exception.GetType().Name;

        return message.Length <= MaxSanitizedErrorLength
            ? message
            : message[..MaxSanitizedErrorLength];
    }
}
