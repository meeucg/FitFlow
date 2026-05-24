namespace ApiGateway.Application.Abstractions;

public interface IRecommendationNotifier
{
    void PublishBatch(Guid userId, IReadOnlyList<Guid> ids, DateTimeOffset latestRecommendationAt);

    void PublishSingle(Guid userId, Guid id, DateTimeOffset recommendedAt);
}
