using Microsoft.Extensions.Options;

namespace ApiGateway.Options;

/// <summary>
/// Recommendation flow options for ApiGateway.
/// </summary>
public sealed class RecommendationsOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Recommendations";

    /// <summary>
    /// Background starter initialization poll interval.
    /// </summary>
    public TimeSpan InitializationPollInterval { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Delay before retrying failed starter initialization.
    /// </summary>
    public TimeSpan InitializationRetryDelay { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum starter initialization retry count.
    /// </summary>
    public int InitializationMaxRetries { get; init; } = 10;

    /// <summary>
    /// SSE heartbeat interval.
    /// </summary>
    public TimeSpan SseHeartbeatInterval { get; init; } = TimeSpan.FromSeconds(15);

}

/// <summary>
/// Validates recommendation flow options.
/// </summary>
public sealed class RecommendationsOptionsValidator : IValidateOptions<RecommendationsOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, RecommendationsOptions options)
    {
        if (options.InitializationPollInterval <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Recommendations:InitializationPollInterval must be positive.");

        if (options.InitializationRetryDelay <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Recommendations:InitializationRetryDelay must be positive.");

        if (options.InitializationMaxRetries <= 0)
            return ValidateOptionsResult.Fail("Recommendations:InitializationMaxRetries must be positive.");

        if (options.SseHeartbeatInterval <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Recommendations:SseHeartbeatInterval must be positive.");

        return ValidateOptionsResult.Success;
    }
}
