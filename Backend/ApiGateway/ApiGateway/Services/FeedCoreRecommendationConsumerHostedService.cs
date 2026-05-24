using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiGateway.Application.Recommendations;
using ApiGateway.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ApiGateway.Services;

/// <summary>
/// Consumes live recommendation events published by FeedCore.
/// </summary>
public sealed class FeedCoreRecommendationConsumerHostedService(
    IServiceScopeFactory scopeFactory,
    RecommendationSseHub sseHub,
    IOptions<RabbitMqOptions> options,
    ILogger<FeedCoreRecommendationConsumerHostedService> logger) : BackgroundService
{
    private readonly JsonSerializerOptions jsonOptions = CreateJsonOptions();

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbitMqOptions = options.Value;
        var factory = CreateFactory(rabbitMqOptions);
        await using var connection = await ConnectWithRetryAsync(factory, stoppingToken);
        var channels = new List<IChannel>();

        try
        {
            await using var topologyChannel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
            await DeclareTopologyAsync(topologyChannel, rabbitMqOptions, stoppingToken);

            for (var index = 0; index < rabbitMqOptions.ConsumerCount; index++)
            {
                var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
                channels.Add(channel);

                await channel.BasicQosAsync(
                    prefetchSize: 0,
                    prefetchCount: rabbitMqOptions.PrefetchCount,
                    global: false,
                    cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, eventArgs) =>
                    await ProcessDeliveryAsync(channel, eventArgs, stoppingToken);

                await channel.BasicConsumeAsync(
                    queue: rabbitMqOptions.RecommendationsQueue,
                    autoAck: false,
                    consumerTag: $"api-gateway-feedcore-recommendations-{index + 1}",
                    consumer: consumer,
                    cancellationToken: stoppingToken);
            }

            logger.LogInformation(
                "ApiGateway FeedCore recommendation consumers started. Consumers={ConsumerCount}, Queue={Queue}",
                rabbitMqOptions.ConsumerCount,
                rabbitMqOptions.RecommendationsQueue);

            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("ApiGateway FeedCore recommendation consumers are stopping.");
        }
        finally
        {
            foreach (var channel in channels)
                await channel.DisposeAsync();
        }
    }

    private async Task ProcessDeliveryAsync(
        IChannel channel,
        BasicDeliverEventArgs eventArgs,
        CancellationToken cancellationToken)
    {
        RecommendationCreatedMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<RecommendationCreatedMessage>(
                Encoding.UTF8.GetString(eventArgs.Body.Span),
                jsonOptions);

            if (message is null || message.JobPostingId == Guid.Empty)
                throw new JsonException("Recommendation event is missing job_posting_id.");
        }
        catch (JsonException exception)
        {
            logger.LogWarning(
                "Rejected malformed FeedCore recommendation event. DeliveryTag={DeliveryTag}, Error={Error}",
                eventArgs.DeliveryTag,
                exception.Message);

            await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken);
            return;
        }

        try
        {
            using var scope = scopeFactory.CreateScope();
            var liveRecommendations = scope.ServiceProvider.GetRequiredService<LiveRecommendationService>();
            var stored = await liveRecommendations.StoreAsync(
                new StoreLiveRecommendationCommand(message.JobPostingId, message.UserIds),
                cancellationToken);

            await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken);

            foreach (var userId in stored.UserIds)
                sseHub.PublishSingle(userId, stored.JobPostingId, stored.RecommendedAt);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(
                exception,
                "Failed to persist FeedCore recommendation event. JobPostingId={JobPostingId}",
                message.JobPostingId);

            await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken);
        }
    }

    private async Task<IConnection> ConnectWithRetryAsync(ConnectionFactory factory, CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                return await factory.CreateConnectionAsync(cancellationToken);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(
                    exception,
                    "RabbitMQ connection failed. Retrying in 5 seconds. Host={Host}, Port={Port}",
                    factory.HostName,
                    factory.Port);

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }

    private static async Task DeclareTopologyAsync(
        IChannel channel,
        RabbitMqOptions options,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            options.RecommendationsExchange,
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
            options.RecommendationsRoutingKey,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            options.RecommendationsQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = options.DeadLetterExchange,
                ["x-dead-letter-routing-key"] = options.RecommendationsRoutingKey
            },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            options.RecommendationsQueue,
            options.RecommendationsExchange,
            options.RecommendationsRoutingKey,
            cancellationToken: cancellationToken);
    }

    private static ConnectionFactory CreateFactory(RabbitMqOptions options)
        => new()
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.Username,
            Password = options.Password,
            AutomaticRecoveryEnabled = true
        };

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

    private sealed record RecommendationCreatedMessage(Guid JobPostingId, IReadOnlyList<Guid> UserIds);
}
