using FeedCore.Infrastructure.Options;
using RabbitMQ.Client;

namespace FeedCore.Infrastructure.Messaging;

public static class RabbitMqTopology
{
    public static async Task DeclareAsync(
        IChannel channel,
        RabbitMqOptions options,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            options.IncomingNormalizedPostingsExchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            options.OutgoingRecommendationsExchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            options.DeadLetterExchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            options.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            options.DeadLetterQueue,
            options.DeadLetterExchange,
            options.IncomingNormalizedPostingsRoutingKey,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            options.IncomingNormalizedPostingsQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = options.DeadLetterExchange,
                ["x-dead-letter-routing-key"] = options.IncomingNormalizedPostingsRoutingKey
            },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            options.IncomingNormalizedPostingsQueue,
            options.IncomingNormalizedPostingsExchange,
            options.IncomingNormalizedPostingsRoutingKey,
            cancellationToken: cancellationToken);
    }
}
