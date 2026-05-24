namespace ApiGateway.Core.Entities;

/// <summary>
/// Local ApiGateway user record linked to a Keycloak subject and at most one interview.
/// </summary>
public sealed class User
{
    /// <summary>
    /// ApiGateway local user identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Stable Keycloak subject claim that owns this local user row.
    /// </summary>
    public required string KeycloakSubject { get; set; }

    /// <summary>
    /// Email address synchronized from Keycloak claims.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Optional first name synchronized from Keycloak claims.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Optional last name synchronized from Keycloak claims.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Hidden InterviewService interview id owned by this user.
    /// </summary>
    public Guid? CurrentInterviewId { get; set; }

    /// <summary>
    /// Current starter recommendation initialization state owned by ApiGateway.
    /// </summary>
    public RecommendationInitializationState RecommendationState { get; set; } = RecommendationInitializationState.NotStarted;

    /// <summary>
    /// UTC timestamp for when starter recommendation initialization was requested or retried.
    /// </summary>
    public DateTimeOffset? RecommendationRequestedAt { get; set; }

    /// <summary>
    /// UTC timestamp for when starter recommendations became ready.
    /// </summary>
    public DateTimeOffset? RecommendationInitializedAt { get; set; }

    /// <summary>
    /// Number of background starter initialization failures.
    /// </summary>
    public int RecommendationRetryCount { get; set; }

    /// <summary>
    /// Last sanitized starter initialization failure details.
    /// </summary>
    public string? RecommendationLastError { get; set; }

    /// <summary>
    /// UTC timestamp for when this user row was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// UTC timestamp for when this user row was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// ApiGateway-owned starter recommendation initialization state.
/// </summary>
public enum RecommendationInitializationState
{
    /// <summary>
    /// Starter recommendations have not been requested yet.
    /// </summary>
    NotStarted,

    /// <summary>
    /// Starter recommendations are waiting for background initialization.
    /// </summary>
    Pending,

    /// <summary>
    /// Starter recommendations have been initialized.
    /// </summary>
    Ready,

    /// <summary>
    /// Starter recommendation initialization failed and may be retried.
    /// </summary>
    Failed
}
