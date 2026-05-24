namespace ApiGateway.Core.Entities;

/// <summary>
/// ApiGateway-owned state row showing that a job posting was recommended to a local user.
/// </summary>
public sealed class JobRecommendation
{
    /// <summary>
    /// Recommendation row identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Local ApiGateway user id.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// FeedCore job posting id.
    /// </summary>
    public Guid JobPostingId { get; set; }

    /// <summary>
    /// UTC timestamp assigned by ApiGateway when the recommendation became visible to the user.
    /// </summary>
    public DateTimeOffset RecommendedAt { get; set; }

    /// <summary>
    /// Recommendation source: starter or live.
    /// </summary>
    public RecommendationSource Source { get; set; }

    /// <summary>
    /// Recommended user navigation property.
    /// </summary>
    public User? User { get; set; }
}

/// <summary>
/// Origin of a stored recommendation row.
/// </summary>
public enum RecommendationSource
{
    /// <summary>
    /// Recommendation returned by FeedCore.AddNewUser.
    /// </summary>
    Starter,

    /// <summary>
    /// Recommendation produced for a newly arrived job posting.
    /// </summary>
    Live
}
