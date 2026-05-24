using System.Collections.Concurrent;
using System.Threading.Channels;
using ApiGateway.Application.Abstractions;
using ApiGateway.Application.Models;

namespace ApiGateway.Services;

/// <summary>
/// In-memory fan-out hub for active per-user recommendation SSE streams.
/// </summary>
public sealed class RecommendationSseHub : IRecommendationNotifier
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, Channel<SseRecommendationEvent>>> connections = new();

    /// <summary>
    /// Subscribes an active SSE stream for a local user id.
    /// </summary>
    /// <param name="userId">Local user id.</param>
    /// <returns>A disposable subscription with a channel reader.</returns>
    public SseRecommendationSubscription Subscribe(Guid userId)
    {
        var id = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<SseRecommendationEvent>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

        var userConnections = connections.GetOrAdd(userId, _ => new ConcurrentDictionary<Guid, Channel<SseRecommendationEvent>>());
        userConnections[id] = channel;

        return new SseRecommendationSubscription(
            channel.Reader,
            () =>
            {
                if (connections.TryGetValue(userId, out var currentConnections))
                {
                    currentConnections.TryRemove(id, out _);
                    if (currentConnections.IsEmpty)
                        connections.TryRemove(userId, out _);
                }

                channel.Writer.TryComplete();
            });
    }

    /// <summary>
    /// Publishes a batch recommendation command to all active streams for a user.
    /// </summary>
    public void PublishBatch(Guid userId, IReadOnlyList<Guid> ids, DateTimeOffset latestRecommendationAt)
    {
        Publish(userId, SseRecommendationEvent.Batch(ids, latestRecommendationAt));
    }

    /// <summary>
    /// Publishes a single recommendation command to all active streams for a user.
    /// </summary>
    public void PublishSingle(Guid userId, Guid id, DateTimeOffset recommendedAt)
    {
        Publish(userId, SseRecommendationEvent.Single(id, recommendedAt));
    }

    private void Publish(Guid userId, SseRecommendationEvent recommendationEvent)
    {
        if (!connections.TryGetValue(userId, out var userConnections))
            return;

        foreach (var channel in userConnections.Values)
            channel.Writer.TryWrite(recommendationEvent);
    }
}

/// <summary>
/// Active recommendation SSE subscription.
/// </summary>
public sealed class SseRecommendationSubscription(
    ChannelReader<SseRecommendationEvent> reader,
    Action dispose) : IDisposable
{
    /// <summary>
    /// Reads events for the active stream.
    /// </summary>
    public ChannelReader<SseRecommendationEvent> Reader { get; } = reader;

    /// <inheritdoc />
    public void Dispose()
    {
        dispose();
    }
}

/// <summary>
/// Event ready to be serialized as an SSE data frame.
/// </summary>
/// <param name="LatestRecommendationAt">Latest recommendation timestamp carried in the SSE id field.</param>
/// <param name="Command">Command payload serialized in the SSE data field.</param>
public sealed record SseRecommendationEvent(DateTimeOffset LatestRecommendationAt, RecommendationSseCommandDto Command)
{
    internal static SseRecommendationEvent Batch(IReadOnlyList<Guid> ids, DateTimeOffset latestRecommendationAt)
        => new(
            latestRecommendationAt,
            new RecommendationSseCommandDto(
                "send_batch",
                new RecommendationBatchDto(ids.Select(x => x.ToString("D")).ToList())));

    internal static SseRecommendationEvent Single(Guid id, DateTimeOffset recommendedAt)
        => new(
            recommendedAt,
            new RecommendationSseCommandDto(
                "send_single",
                new RecommendationSingleDto(id.ToString("D"))));
}
