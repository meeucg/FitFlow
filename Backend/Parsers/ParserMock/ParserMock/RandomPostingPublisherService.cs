using System.Text.Json.Nodes;

namespace ParserMock;

public sealed class RandomPostingPublisherService(
    IReadOnlyList<JsonObject> postings,
    RabbitMqSettings settings,
    RabbitMqRawPostingPublisher publisher,
    ILogger<RandomPostingPublisherService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "ParserMock loaded {PostingCount} postings and publishes one message every {PublishIntervalSeconds}s.",
            postings.Count,
            settings.PublishInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            var posting = PostingPoolLoader.PickRandom(postings, Random.Shared);
            await publisher.PublishAsync(posting, stoppingToken);

            logger.LogInformation(
                "Published {Source} {Url}",
                posting["source"]?.GetValue<string>(),
                posting["url"]?.GetValue<string>());

            await Task.Delay(settings.PublishInterval, stoppingToken);
        }
    }
}
