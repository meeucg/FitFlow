namespace FeedCore.Application.Models;

public sealed record OutboxMessage(
    Guid Id,
    string Exchange,
    string RoutingKey,
    string BodyJson);
