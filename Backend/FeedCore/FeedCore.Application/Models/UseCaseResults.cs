namespace FeedCore.Application.Models;

public sealed record AcceptNormalizedJobPostingResult(Guid JobPostingId, bool Created);

public sealed record EmbedPendingJobPostingResult(
    int Claimed,
    int Embedded,
    int Failed,
    int RecommendationsCreated);
