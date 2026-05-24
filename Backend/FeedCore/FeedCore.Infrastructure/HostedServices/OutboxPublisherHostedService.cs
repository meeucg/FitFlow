using System.Text;
using FeedCore.Application.Abstractions;
using FeedCore.Application.Models;
using FeedCore.Infrastructure.Messaging;
using FeedCore.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FeedCore.Infrastructure.HostedServices;

public sealed class OutboxPublisherHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    TimeProvider timeProvider,
    ILogger<OutboxPublisherHostedService> logger) : BackgroundService
{
    private const int BatchSize = 50;
    private const int MaxSanitizedErrorLength = 1000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RecoverProcessingOutboxAsync(stoppingToken);

        var rabbitMqOptions = options.Value;
        var factory = CreateFactory(rabbitMqOptions);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = await ConnectWithRetryAsync(factory, stoppingToken);
                await using var channel = await connection.CreateChannelAsync(
                    new CreateChannelOptions(true, true, null, null),
                    stoppingToken);

                await RabbitMqTopology.DeclareAsync(channel, rabbitMqOptions, stoppingToken);
                await PublishLoopAsync(channel, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox publisher connection loop failed. Reconnecting shortly.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task PublishLoopAsync(IChannel channel, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            IReadOnlyList<OutboxMessage> messages;
            using (var scope = scopeFactory.CreateScope())
            {
                var store = scope.ServiceProvider.GetRequiredService<IFeedCoreStore>();
                messages = await store.ClaimPendingOutboxMessagesAsync(
                    BatchSize,
                    timeProvider.GetUtcNow(),
                    cancellationToken);
            }

            if (messages.Count == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                continue;
            }

            foreach (var message in messages)
                await PublishMessageAsync(channel, message, cancellationToken);
        }
    }

    private async Task PublishMessageAsync(
        IChannel channel,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            await channel.BasicPublishAsync(
                exchange: message.Exchange,
                routingKey: message.RoutingKey,
                mandatory: false,
                basicProperties: CreateJsonProperties(message.Id),
                body: Encoding.UTF8.GetBytes(message.BodyJson),
                cancellationToken: cancellationToken);

            using var scope = scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<IFeedCoreStore>();
            await store.MarkOutboxPublishedAsync(message.Id, timeProvider.GetUtcNow(), cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Failed to publish FeedCore outbox message. MessageId={MessageId}", message.Id);

            using var scope = scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<IFeedCoreStore>();
            await store.MarkOutboxFailedAsync(
                message.Id,
                Sanitize(exception),
                timeProvider.GetUtcNow() + TimeSpan.FromSeconds(30),
                cancellationToken);
        }
    }

    private async Task RecoverProcessingOutboxAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<IFeedCoreStore>();
            var recovered = await store.RecoverProcessingOutboxMessagesAsync(timeProvider.GetUtcNow(), cancellationToken);

            if (recovered > 0)
                logger.LogInformation("Recovered interrupted outbox publish attempts. Count={Recovered}", recovered);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Failed to recover interrupted outbox publish attempts.");
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

    private static BasicProperties CreateJsonProperties(Guid messageId)
        => new()
        {
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            Persistent = true,
            MessageId = messageId.ToString("N"),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

    private static ConnectionFactory CreateFactory(RabbitMqOptions options)
        => new()
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.Username,
            Password = options.Password,
            AutomaticRecoveryEnabled = true
        };

    private static string Sanitize(Exception exception)
    {
        var message = exception.GetBaseException().Message;
        if (string.IsNullOrWhiteSpace(message))
            message = exception.GetType().Name;

        return message.Length <= MaxSanitizedErrorLength
            ? message
            : message[..MaxSanitizedErrorLength];
    }
}
