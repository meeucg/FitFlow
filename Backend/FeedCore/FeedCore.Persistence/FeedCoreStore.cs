using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using FeedCore.Application.Abstractions;
using FeedCore.Application.Exceptions;
using FeedCore.Application.Models;
using FeedCore.Core.Models;
using FeedCore.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedCore.Persistence;

public sealed class FeedCoreStore(FeedCoreDbContext dbContext) : IFeedCoreStore
{
    private static readonly JsonSerializerOptions DisplayJsonOptions = CreateJsonOptions();

    public async Task<IReadOnlyList<Guid>> UpsertUserEmbeddingAndFindStarterRecommendationsAsync(
        Guid userId,
        EmbeddingVector embedding,
        DateTimeOffset postedAfter,
        double maxCosineDistance,
        int limit,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var vector = ToVectorLiteral(embedding);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO user_profile_embeddings (user_id, embedding, created_at, updated_at)
            VALUES ({userId}, CAST({vector} AS vector), {now}, {now})
            ON CONFLICT (user_id) DO UPDATE
            SET embedding = CAST({vector} AS vector),
                updated_at = EXCLUDED.updated_at
            """, cancellationToken);

        var matches = await dbContext.Database.SqlQuery<Guid>($"""
            SELECT id AS "Value"
            FROM job_postings
            WHERE embedding IS NOT NULL
              AND embedding_state = 'embedded'
              AND posted_at >= {postedAfter}
              AND (embedding <=> CAST({vector} AS vector)) <= {maxCosineDistance}
            ORDER BY (embedding <=> CAST({vector} AS vector)) ASC, posted_at DESC
            LIMIT {limit}
            """).ToListAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return matches;
    }

    public async Task<AcceptNormalizedJobPostingResult> SaveNormalizedPostingAsync(
        NormalizedJobPostingInput posting,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var display = posting.DisplayData with { Id = id };
        var displayJson = JsonSerializer.Serialize(display, DisplayJsonOptions);

        var inserted = await dbContext.Database.SqlQuery<Guid>($"""
            INSERT INTO job_postings (
                id,
                source,
                url,
                posted_at,
                received_at,
                display_json,
                embedding_state,
                embedding_attempts)
            VALUES (
                {id},
                {posting.Source.Trim()},
                {posting.Url.Trim()},
                {posting.PostedAt},
                {now},
                CAST({displayJson} AS jsonb),
                'pending',
                0)
            ON CONFLICT (source, url) DO NOTHING
            RETURNING id AS "Value"
            """).ToListAsync(cancellationToken);

        if (inserted.Count > 0)
            return new AcceptNormalizedJobPostingResult(inserted[0], Created: true);

        var existingId = await dbContext.JobPostings
            .Where(x => x.Source == posting.Source.Trim() && x.Url == posting.Url.Trim())
            .Select(x => x.Id)
            .SingleAsync(cancellationToken);

        return new AcceptNormalizedJobPostingResult(existingId, Created: false);
    }

    public async Task<IReadOnlyList<PendingJobPosting>> ClaimPendingJobPostingsAsync(
        int limit,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var ids = await dbContext.Database.SqlQuery<Guid>($"""
            SELECT id AS "Value"
            FROM job_postings
            WHERE embedding_state IN ('pending', 'failed')
              AND (next_embedding_attempt_at IS NULL OR next_embedding_attempt_at <= {now})
            ORDER BY received_at ASC
            LIMIT {limit}
            FOR UPDATE SKIP LOCKED
            """).ToListAsync(cancellationToken);

        if (ids.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return [];
        }

        await dbContext.JobPostings
            .Where(x => ids.Contains(x.Id))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.EmbeddingState, EmbeddingState.Processing)
                    .SetProperty(x => x.EmbeddingAttempts, x => x.EmbeddingAttempts + 1)
                    .SetProperty(x => x.NextEmbeddingAttemptAt, (DateTimeOffset?)null),
                cancellationToken);

        var entities = await dbContext.JobPostings
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return entities
            .Select(ToPendingJobPosting)
            .ToList();
    }

    public async Task<IReadOnlyList<Guid>> FindMatchingUserIdsAsync(
        EmbeddingVector embedding,
        double maxCosineDistance,
        int limit,
        CancellationToken cancellationToken)
    {
        var vector = ToVectorLiteral(embedding);

        var ids = await dbContext.Database.SqlQuery<Guid>($"""
            SELECT user_id AS "Value"
            FROM user_profile_embeddings
            WHERE (embedding <=> CAST({vector} AS vector)) <= {maxCosineDistance}
            ORDER BY (embedding <=> CAST({vector} AS vector)) ASC, updated_at DESC
            LIMIT {limit}
            """).ToListAsync(cancellationToken);

        return ids;
    }

    public async Task CompleteJobEmbeddingAsync(
        Guid jobPostingId,
        EmbeddingVector embedding,
        IReadOnlyList<Guid> matchedUserIds,
        string recommendationsExchange,
        string recommendationsRoutingKey,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var vector = ToVectorLiteral(embedding);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE job_postings
            SET embedding = CAST({vector} AS vector),
                embedding_state = 'embedded',
                embedded_at = {now},
                next_embedding_attempt_at = NULL,
                last_embedding_error = NULL
            WHERE id = {jobPostingId}
            """, cancellationToken);

        if (matchedUserIds.Count > 0)
        {
            var body = JsonSerializer.Serialize(
                new RecommendationCreatedBody(jobPostingId, matchedUserIds),
                DisplayJsonOptions);

            dbContext.RecommendationOutboxMessages.Add(new RecommendationOutboxMessageEntity
            {
                Id = Guid.NewGuid(),
                Exchange = recommendationsExchange,
                RoutingKey = recommendationsRoutingKey,
                BodyJson = body,
                OccurredAt = now,
                Status = OutboxMessageStatus.Pending,
                Attempts = 0,
                NextAttemptAt = now
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public Task MarkJobEmbeddingFailedAsync(
        Guid jobPostingId,
        string sanitizedError,
        DateTimeOffset nextAttemptAt,
        CancellationToken cancellationToken)
        => dbContext.JobPostings
            .Where(x => x.Id == jobPostingId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.EmbeddingState, EmbeddingState.Failed)
                    .SetProperty(x => x.NextEmbeddingAttemptAt, nextAttemptAt)
                    .SetProperty(x => x.LastEmbeddingError, sanitizedError),
                cancellationToken);

    public Task<int> RecoverProcessingJobPostingsAsync(DateTimeOffset now, CancellationToken cancellationToken)
        => dbContext.JobPostings
            .Where(x => x.EmbeddingState == EmbeddingState.Processing)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.EmbeddingState, EmbeddingState.Failed)
                    .SetProperty(x => x.NextEmbeddingAttemptAt, now)
                    .SetProperty(x => x.LastEmbeddingError, "Recovered from interrupted embedding attempt."),
                cancellationToken);

    public async Task<JobPostingDisplayData?> GetJobPostingDisplayAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.JobPostings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
            return null;

        var display = JsonSerializer.Deserialize<JobPostingDisplayData>(entity.DisplayJson, DisplayJsonOptions)
                      ?? throw new FeedCorePersistenceException("Stored job posting display JSON could not be deserialized.");

        return display with
        {
            Id = entity.Id,
            Source = entity.Source,
            Url = entity.Url,
            PostedAt = entity.PostedAt
        };
    }

    public async Task<IReadOnlyList<OutboxMessage>> ClaimPendingOutboxMessagesAsync(
        int limit,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var ids = await dbContext.Database.SqlQuery<Guid>($"""
            SELECT id AS "Value"
            FROM recommendation_outbox_messages
            WHERE status IN ('pending', 'failed')
              AND next_attempt_at <= {now}
            ORDER BY occurred_at ASC
            LIMIT {limit}
            FOR UPDATE SKIP LOCKED
            """).ToListAsync(cancellationToken);

        if (ids.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return [];
        }

        await dbContext.RecommendationOutboxMessages
            .Where(x => ids.Contains(x.Id))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, OutboxMessageStatus.Processing)
                    .SetProperty(x => x.Attempts, x => x.Attempts + 1)
                    .SetProperty(x => x.LastError, (string?)null),
                cancellationToken);

        var entities = await dbContext.RecommendationOutboxMessages
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return entities
            .Select(x => new OutboxMessage(x.Id, x.Exchange, x.RoutingKey, x.BodyJson))
            .ToList();
    }

    public Task MarkOutboxPublishedAsync(Guid id, DateTimeOffset now, CancellationToken cancellationToken)
        => dbContext.RecommendationOutboxMessages
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, OutboxMessageStatus.Published)
                    .SetProperty(x => x.PublishedAt, now)
                    .SetProperty(x => x.LastError, (string?)null),
                cancellationToken);

    public Task MarkOutboxFailedAsync(
        Guid id,
        string sanitizedError,
        DateTimeOffset nextAttemptAt,
        CancellationToken cancellationToken)
        => dbContext.RecommendationOutboxMessages
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, OutboxMessageStatus.Failed)
                    .SetProperty(x => x.NextAttemptAt, nextAttemptAt)
                    .SetProperty(x => x.LastError, sanitizedError),
                cancellationToken);

    public Task<int> RecoverProcessingOutboxMessagesAsync(DateTimeOffset now, CancellationToken cancellationToken)
        => dbContext.RecommendationOutboxMessages
            .Where(x => x.Status == OutboxMessageStatus.Processing)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, OutboxMessageStatus.Failed)
                    .SetProperty(x => x.NextAttemptAt, now)
                    .SetProperty(x => x.LastError, "Recovered from interrupted publish attempt."),
                cancellationToken);

    private static PendingJobPosting ToPendingJobPosting(JobPostingEntity entity)
    {
        var display = JsonSerializer.Deserialize<JobPostingDisplayData>(entity.DisplayJson, DisplayJsonOptions)
                      ?? throw new FeedCorePersistenceException("Stored job posting display JSON could not be deserialized.");

        return new PendingJobPosting(
            entity.Id,
            display with
            {
                Id = entity.Id,
                Source = entity.Source,
                Url = entity.Url,
                PostedAt = entity.PostedAt
            });
    }

    private static string ToVectorLiteral(EmbeddingVector embedding)
    {
        if (embedding.Values.Length == 0)
            throw new FeedCoreValidationException("Embedding vector must not be empty.");

        return "[" + string.Join(
            ',',
            embedding.Values.Select(static value =>
            {
                if (!float.IsFinite(value))
                    throw new FeedCoreValidationException("Embedding vector contains a non-finite value.");

                return value.ToString("R", CultureInfo.InvariantCulture);
            })) + "]";
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
        return options;
    }

    private sealed record RecommendationCreatedBody(
        Guid JobPostingId,
        IReadOnlyList<Guid> UserIds);
}
