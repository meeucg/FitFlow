namespace FeedCore.Persistence.Entities;

public sealed class RecommendationOutboxMessageEntity
{
    public Guid Id { get; set; }
    public required string Exchange { get; set; }
    public required string RoutingKey { get; set; }
    public required string BodyJson { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public OutboxMessageStatus Status { get; set; }
    public int Attempts { get; set; }
    public DateTimeOffset NextAttemptAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public string? LastError { get; set; }
}

public enum OutboxMessageStatus
{
    Pending,
    Processing,
    Published,
    Failed
}
