using Microsoft.Extensions.Options;

namespace FeedCore.Infrastructure.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string IncomingNormalizedPostingsExchange { get; init; } = "raw-postings-filter.outgoing.mock";
    public string IncomingNormalizedPostingsQueue { get; init; } = "feed-core.normalized-postings";
    public string IncomingNormalizedPostingsRoutingKey { get; init; } = "job-posting.normalized";
    public string OutgoingRecommendationsExchange { get; init; } = "feed-core.recommendations";
    public string OutgoingRecommendationsRoutingKey { get; init; } = "recommendation.created";
    public string DeadLetterExchange { get; init; } = "feed-core.dead-letter";
    public string DeadLetterQueue { get; init; } = "feed-core.dead-letter";
    public int ConsumerCount { get; init; } = 2;
    public ushort PrefetchCount { get; init; } = 1;
}

public sealed class RabbitMqOptionsValidator : IValidateOptions<RabbitMqOptions>
{
    public ValidateOptionsResult Validate(string? name, RabbitMqOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Host))
            return ValidateOptionsResult.Fail("RabbitMq:Host is required.");

        if (options.Port <= 0)
            return ValidateOptionsResult.Fail("RabbitMq:Port must be positive.");

        if (string.IsNullOrWhiteSpace(options.IncomingNormalizedPostingsExchange))
            return ValidateOptionsResult.Fail("RabbitMq:IncomingNormalizedPostingsExchange is required.");

        if (string.IsNullOrWhiteSpace(options.IncomingNormalizedPostingsQueue))
            return ValidateOptionsResult.Fail("RabbitMq:IncomingNormalizedPostingsQueue is required.");

        if (string.IsNullOrWhiteSpace(options.OutgoingRecommendationsExchange))
            return ValidateOptionsResult.Fail("RabbitMq:OutgoingRecommendationsExchange is required.");

        if (options.ConsumerCount <= 0)
            return ValidateOptionsResult.Fail("RabbitMq:ConsumerCount must be positive.");

        if (options.PrefetchCount <= 0)
            return ValidateOptionsResult.Fail("RabbitMq:PrefetchCount must be positive.");

        return ValidateOptionsResult.Success;
    }
}
