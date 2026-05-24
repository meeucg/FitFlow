using System.Globalization;
using ApiGateway.Application.Abstractions;
using ApiGateway.Core;
using ApiGateway.Core.Entities;

namespace ApiGateway.Application.Recommendations;

public sealed class RecommendationSnapshotService(IJobRecommendationRepository recommendations)
{
    public static RecommendationCursorKind ParseCursor(string? cursor, out DateTimeOffset? parsed)
    {
        parsed = null;
        if (string.IsNullOrWhiteSpace(cursor) || string.Equals(cursor, "null", StringComparison.OrdinalIgnoreCase))
            return RecommendationCursorKind.FreshPostInterview;

        if (!DateTimeOffset.TryParse(
                cursor,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var timestamp))
        {
            return RecommendationCursorKind.Invalid;
        }

        parsed = timestamp;

        return timestamp == DateTimeOffset.MinValue
            ? RecommendationCursorKind.ZeroTimestamp
            : RecommendationCursorKind.Timestamp;
    }

    public async Task<RecommendationBatch?> GetInitialBatchAsync(
        RecommendationCursorKind cursorKind,
        DateTimeOffset? cursor,
        User user,
        CancellationToken cancellationToken)
    {
        if (cursorKind == RecommendationCursorKind.FreshPostInterview)
        {
            if (user.RecommendationState != RecommendationInitializationState.Ready)
                return null;

            var starterRecommendations = await recommendations.ListStarterAsync(user.Id, cancellationToken);
            var latest = starterRecommendations.Count > 0
                ? starterRecommendations.Max(x => x.RecommendedAt)
                : user.RecommendationInitializedAt ?? DateTimeOffset.UtcNow;

            return new RecommendationBatch(
                starterRecommendations.Select(x => x.JobPostingId).ToList(),
                latest);
        }

        if (cursorKind == RecommendationCursorKind.ZeroTimestamp)
        {
            var allRecommendations = await recommendations.ListAllAsync(user.Id, cancellationToken);

            if (allRecommendations.Count == 0 && user.RecommendationState != RecommendationInitializationState.Ready)
                return null;

            var latest = allRecommendations.Count > 0
                ? allRecommendations.Max(x => x.RecommendedAt)
                : user.RecommendationInitializedAt ?? DateTimeOffset.UtcNow;

            return new RecommendationBatch(
                allRecommendations.Select(x => x.JobPostingId).ToList(),
                latest);
        }

        if (cursor is null)
            return null;

        var updatedRecommendations = await recommendations.ListAfterAsync(user.Id, cursor.Value, cancellationToken);
        if (updatedRecommendations.Count == 0)
            return null;

        return new RecommendationBatch(
            updatedRecommendations.Select(x => x.JobPostingId).ToList(),
            updatedRecommendations.Max(x => x.RecommendedAt));
    }
}

public sealed record RecommendationBatch(
    IReadOnlyList<Guid> JobPostingIds,
    DateTimeOffset LatestRecommendationAt);

public enum RecommendationCursorKind
{
    FreshPostInterview,
    ZeroTimestamp,
    Timestamp,
    Invalid
}
