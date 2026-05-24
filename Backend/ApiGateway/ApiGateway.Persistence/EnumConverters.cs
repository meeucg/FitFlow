using ApiGateway.Core;
using ApiGateway.Core.Entities;

namespace ApiGateway.Persistence;

internal static class RecommendationInitializationStateConverter
{
    public static string ToDatabase(RecommendationInitializationState state)
        => state switch
        {
            RecommendationInitializationState.NotStarted => "not_started",
            RecommendationInitializationState.Pending => "pending",
            RecommendationInitializationState.Ready => "ready",
            RecommendationInitializationState.Failed => "failed",
            _ => "not_started"
        };

    public static RecommendationInitializationState FromDatabase(string value)
        => value switch
        {
            "not_started" => RecommendationInitializationState.NotStarted,
            "pending" => RecommendationInitializationState.Pending,
            "ready" => RecommendationInitializationState.Ready,
            "failed" => RecommendationInitializationState.Failed,
            _ => RecommendationInitializationState.NotStarted
        };
}

internal static class RecommendationSourceConverter
{
    public static string ToDatabase(RecommendationSource source)
        => source switch
        {
            RecommendationSource.Starter => "starter",
            RecommendationSource.Live => "live",
            _ => "live"
        };

    public static RecommendationSource FromDatabase(string value)
        => value switch
        {
            "starter" => RecommendationSource.Starter,
            "live" => RecommendationSource.Live,
            _ => RecommendationSource.Live
        };
}
