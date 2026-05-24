using FeedCore.Core.Models;

namespace FeedCore.Persistence.Entities;

public sealed class JobPostingEntity
{
    public Guid Id { get; set; }
    public required string Source { get; set; }
    public required string Url { get; set; }
    public DateTimeOffset PostedAt { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
    public required string DisplayJson { get; set; }
    public EmbeddingState EmbeddingState { get; set; }
    public int EmbeddingAttempts { get; set; }
    public DateTimeOffset? EmbeddedAt { get; set; }
    public DateTimeOffset? NextEmbeddingAttemptAt { get; set; }
    public string? LastEmbeddingError { get; set; }
}
