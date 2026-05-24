using FeedCore.Application.Abstractions;
using FeedCore.Application.Exceptions;
using FeedCore.Application.Models;
using FeedCore.Application.Options;
using FeedCore.Application.Rendering;
using Microsoft.Extensions.Options;

namespace FeedCore.Application.UseCases;

public sealed class AddNewUserUseCase(
    IEmbeddingGenerator embeddingGenerator,
    IFeedCoreStore store,
    InterviewConclusionTextRenderer renderer,
    IOptions<RecommendationOptions> recommendationOptions,
    TimeProvider timeProvider)
{
    public async Task<IReadOnlyList<Guid>> ExecuteAsync(
        string userId,
        InterviewConclusionData conclusion,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userId, out var parsedUserId))
            throw new FeedCoreValidationException("User id must be a valid GUID.");

        if (!HasSemanticContent(conclusion))
            throw new FeedCoreValidationException("Interview conclusion does not contain enough semantic content.");

        var text = renderer.Render(conclusion);
        if (string.IsNullOrWhiteSpace(text))
            throw new FeedCoreValidationException("Interview conclusion rendered to empty embedding text.");

        var embedding = await embeddingGenerator.GenerateAsync(text, cancellationToken);
        var options = recommendationOptions.Value;
        EnsureDimensions(embedding, options.EmbeddingDimensions);

        var now = timeProvider.GetUtcNow();
        var postedAfter = now - options.StarterLookback;

        return await store.UpsertUserEmbeddingAndFindStarterRecommendationsAsync(
            parsedUserId,
            embedding,
            postedAfter,
            options.MatchMaxCosineDistance,
            options.StarterRecommendationLimit,
            now,
            cancellationToken);
    }

    private static bool HasSemanticContent(InterviewConclusionData conclusion)
        => HasText(conclusion.Cluster)
           || conclusion.Specializations.Any(x => HasText(x.Name))
           || conclusion.Skills.Any(x => HasText(x.DisplayName) || HasText(x.Description))
           || conclusion.Tools.Any(x => HasText(x.ToolStandardName))
           || conclusion.PreferredDomains.Any(x => HasText(x.Name));

    private static bool HasText(string? value)
        => !string.IsNullOrWhiteSpace(value);

    private static void EnsureDimensions(EmbeddingVector embedding, int expectedDimensions)
    {
        if (embedding.Dimensions != expectedDimensions)
            throw new EmbeddingProviderException(
                $"Embedding provider returned {embedding.Dimensions} dimensions, expected {expectedDimensions}.");
    }
}
