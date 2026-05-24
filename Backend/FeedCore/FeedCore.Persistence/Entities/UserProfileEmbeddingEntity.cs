namespace FeedCore.Persistence.Entities;

public sealed class UserProfileEmbeddingEntity
{
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
