namespace ApiGateway.Application.Recommendations;

public sealed record StoreLiveRecommendationCommand(Guid JobPostingId, IReadOnlyList<Guid> UserIds);

public sealed record StoredLiveRecommendations(
    Guid JobPostingId,
    IReadOnlyList<Guid> UserIds,
    DateTimeOffset RecommendedAt);
