using Microsoft.Extensions.Options;

namespace FeedCore.Application.Options;

public sealed class RecommendationOptions
{
    public const string SectionName = "Recommendation";

    public int EmbeddingDimensions { get; init; } = 1536;
    public double MatchMaxCosineDistance { get; init; } = 0.35;
    public TimeSpan StarterLookback { get; init; } = TimeSpan.FromDays(30);
    public int StarterRecommendationLimit { get; init; } = 20;
    public int MaxUsersPerJob { get; init; } = 1000;
    public int PendingEmbeddingBatchSize { get; init; } = 25;
    public TimeSpan PendingEmbeddingPollInterval { get; init; } = TimeSpan.FromSeconds(5);
}

public sealed class RecommendationOptionsValidator : IValidateOptions<RecommendationOptions>
{
    public ValidateOptionsResult Validate(string? name, RecommendationOptions options)
    {
        if (options.EmbeddingDimensions != 1536)
            return ValidateOptionsResult.Fail("Recommendation:EmbeddingDimensions must be exactly 1536.");

        if (options.MatchMaxCosineDistance <= 0 || options.MatchMaxCosineDistance >= 2)
            return ValidateOptionsResult.Fail("Recommendation:MatchMaxCosineDistance must be greater than 0 and less than 2.");

        if (options.StarterLookback <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Recommendation:StarterLookback must be positive.");

        if (options.StarterRecommendationLimit <= 0)
            return ValidateOptionsResult.Fail("Recommendation:StarterRecommendationLimit must be positive.");

        if (options.MaxUsersPerJob <= 0)
            return ValidateOptionsResult.Fail("Recommendation:MaxUsersPerJob must be positive.");

        if (options.PendingEmbeddingBatchSize <= 0)
            return ValidateOptionsResult.Fail("Recommendation:PendingEmbeddingBatchSize must be positive.");

        if (options.PendingEmbeddingPollInterval <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Recommendation:PendingEmbeddingPollInterval must be positive.");

        return ValidateOptionsResult.Success;
    }
}
