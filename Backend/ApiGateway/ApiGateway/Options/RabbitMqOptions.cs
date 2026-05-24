using Microsoft.Extensions.Options;

namespace ApiGateway.Options;

/// <summary>
/// RabbitMQ options for consuming FeedCore recommendation events.
/// </summary>
public sealed class RabbitMqOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "RabbitMq";

    /// <summary>
    /// Broker host.
    /// </summary>
    public string Host { get; init; } = "localhost";

    /// <summary>
    /// Broker port.
    /// </summary>
    public int Port { get; init; } = 5672;

    /// <summary>
    /// Broker username.
    /// </summary>
    public string Username { get; init; } = "guest";

    /// <summary>
    /// Broker password.
    /// </summary>
    public string Password { get; init; } = "guest";

    /// <summary>
    /// FeedCore recommendations exchange.
    /// </summary>
    public string RecommendationsExchange { get; init; } = "feed-core.recommendations";

    /// <summary>
    /// ApiGateway recommendations queue.
    /// </summary>
    public string RecommendationsQueue { get; init; } = "api-gateway.recommendations";

    /// <summary>
    /// FeedCore recommendation routing key.
    /// </summary>
    public string RecommendationsRoutingKey { get; init; } = "recommendation.created";

    /// <summary>
    /// ApiGateway dead-letter exchange.
    /// </summary>
    public string DeadLetterExchange { get; init; } = "api-gateway.dead-letter";

    /// <summary>
    /// ApiGateway dead-letter queue.
    /// </summary>
    public string DeadLetterQueue { get; init; } = "api-gateway.dead-letter";

    /// <summary>
    /// Number of recommendation consumers.
    /// </summary>
    public int ConsumerCount { get; init; } = 1;

    /// <summary>
    /// Recommendation consumer prefetch count.
    /// </summary>
    public ushort PrefetchCount { get; init; } = 10;
}

/// <summary>
/// Validates RabbitMQ recommendation consumer options.
/// </summary>
public sealed class RabbitMqOptionsValidator : IValidateOptions<RabbitMqOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, RabbitMqOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Host))
            return ValidateOptionsResult.Fail("RabbitMq:Host is required.");

        if (options.Port <= 0)
            return ValidateOptionsResult.Fail("RabbitMq:Port must be positive.");

        if (string.IsNullOrWhiteSpace(options.RecommendationsExchange))
            return ValidateOptionsResult.Fail("RabbitMq:RecommendationsExchange is required.");

        if (string.IsNullOrWhiteSpace(options.RecommendationsQueue))
            return ValidateOptionsResult.Fail("RabbitMq:RecommendationsQueue is required.");

        if (string.IsNullOrWhiteSpace(options.RecommendationsRoutingKey))
            return ValidateOptionsResult.Fail("RabbitMq:RecommendationsRoutingKey is required.");

        if (options.ConsumerCount <= 0)
            return ValidateOptionsResult.Fail("RabbitMq:ConsumerCount must be positive.");

        if (options.PrefetchCount <= 0)
            return ValidateOptionsResult.Fail("RabbitMq:PrefetchCount must be positive.");

        return ValidateOptionsResult.Success;
    }
}
