using FeedCore.Core.Models;
using FeedCore.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedCore.Persistence;

public sealed class FeedCoreDbContext(DbContextOptions<FeedCoreDbContext> options) : DbContext(options)
{
    public DbSet<JobPostingEntity> JobPostings => Set<JobPostingEntity>();
    public DbSet<UserProfileEmbeddingEntity> UserProfileEmbeddings => Set<UserProfileEmbeddingEntity>();
    public DbSet<RecommendationOutboxMessageEntity> RecommendationOutboxMessages => Set<RecommendationOutboxMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobPostingEntity>(builder =>
        {
            builder.ToTable("job_postings");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Source).HasColumnName("source").HasMaxLength(64).IsRequired();
            builder.Property(x => x.Url).HasColumnName("url").HasMaxLength(2048).IsRequired();
            builder.Property(x => x.PostedAt).HasColumnName("posted_at").HasColumnType("timestamptz").IsRequired();
            builder.Property(x => x.ReceivedAt).HasColumnName("received_at").HasColumnType("timestamptz").IsRequired();
            builder.Property(x => x.DisplayJson).HasColumnName("display_json").HasColumnType("jsonb").IsRequired();
            builder.Property<string?>("Embedding")
                .HasColumnName("embedding")
                .HasColumnType("vector(1536)")
                .IsRequired(false);
            builder.Property(x => x.EmbeddingState)
                .HasColumnName("embedding_state")
                .HasMaxLength(32)
                .HasConversion(
                    state => state.ToString().ToLowerInvariant(),
                    value => Enum.Parse<EmbeddingState>(value, true))
                .IsRequired();
            builder.Property(x => x.EmbeddingAttempts).HasColumnName("embedding_attempts").IsRequired();
            builder.Property(x => x.EmbeddedAt).HasColumnName("embedded_at").HasColumnType("timestamptz");
            builder.Property(x => x.NextEmbeddingAttemptAt).HasColumnName("next_embedding_attempt_at").HasColumnType("timestamptz");
            builder.Property(x => x.LastEmbeddingError).HasColumnName("last_embedding_error");
            builder.HasIndex(x => new { x.Source, x.Url }).IsUnique();
            builder.HasIndex(x => x.PostedAt);
            builder.HasIndex(x => new { x.EmbeddingState, x.NextEmbeddingAttemptAt });
        });

        modelBuilder.Entity<UserProfileEmbeddingEntity>(builder =>
        {
            builder.ToTable("user_profile_embeddings");
            builder.HasKey(x => x.UserId);
            builder.Property(x => x.UserId).HasColumnName("user_id");
            builder.Property<string>("Embedding")
                .HasColumnName("embedding")
                .HasColumnType("vector(1536)")
                .IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz").IsRequired();
        });

        modelBuilder.Entity<RecommendationOutboxMessageEntity>(builder =>
        {
            builder.ToTable("recommendation_outbox_messages");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Exchange).HasColumnName("exchange").HasMaxLength(255).IsRequired();
            builder.Property(x => x.RoutingKey).HasColumnName("routing_key").HasMaxLength(255).IsRequired();
            builder.Property(x => x.BodyJson).HasColumnName("body_json").HasColumnType("jsonb").IsRequired();
            builder.Property(x => x.OccurredAt).HasColumnName("occurred_at").HasColumnType("timestamptz").IsRequired();
            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(32)
                .HasConversion(
                    status => status.ToString().ToLowerInvariant(),
                    value => Enum.Parse<OutboxMessageStatus>(value, true))
                .IsRequired();
            builder.Property(x => x.Attempts).HasColumnName("attempts").IsRequired();
            builder.Property(x => x.NextAttemptAt).HasColumnName("next_attempt_at").HasColumnType("timestamptz").IsRequired();
            builder.Property(x => x.PublishedAt).HasColumnName("published_at").HasColumnType("timestamptz");
            builder.Property(x => x.LastError).HasColumnName("last_error");
            builder.HasIndex(x => new { x.Status, x.NextAttemptAt });
        });
    }
}
