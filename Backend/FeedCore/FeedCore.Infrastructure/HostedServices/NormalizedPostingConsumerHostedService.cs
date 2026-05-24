using System.Text;
using System.Text.Json;
using FeedCore.Application.Exceptions;
using FeedCore.Application.UseCases;
using FeedCore.Infrastructure.Messaging;
using FeedCore.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FeedCore.Infrastructure.HostedServices;

public sealed class NormalizedPostingConsumerHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<NormalizedPostingConsumerHostedService> logger) : BackgroundService
{
    private readonly JsonSerializerOptions _jsonOptions = FeedCoreJsonSerializerOptions.CreateSnakeCase();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbitMqOptions = options.Value;
        var factory = CreateFactory(rabbitMqOptions);

        await using var connection = await ConnectWithRetryAsync(factory, stoppingToken);
        var channels = new List<IChannel>();

        try
        {
            await using var topologyChannel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
            await RabbitMqTopology.DeclareAsync(topologyChannel, rabbitMqOptions, stoppingToken);

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
                var consumerNumber = index + 1;
                consumer.ReceivedAsync += async (_, eventArgs) =>
                    await ProcessDeliveryAsync(channel, eventArgs, consumerNumber, stoppingToken);

                await channel.BasicConsumeAsync(
                    queue: rabbitMqOptions.IncomingNormalizedPostingsQueue,
                    autoAck: false,
                    consumerTag: $"feed-core-normalized-postings-{consumerNumber}",
                    consumer: consumer,
                    cancellationToken: stoppingToken);
            }

            logger.LogInformation(
                "FeedCore normalized posting consumers started. Consumers={ConsumerCount}, Queue={Queue}",
                rabbitMqOptions.ConsumerCount,
                rabbitMqOptions.IncomingNormalizedPostingsQueue);

            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("FeedCore normalized posting consumers are stopping.");
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
        int consumerNumber,
        CancellationToken cancellationToken)
    {
        IncomingNormalizedJobPosting? message;

        try
        {
            message = JsonSerializer.Deserialize<IncomingNormalizedJobPosting>(
                Encoding.UTF8.GetString(eventArgs.Body.Span),
                _jsonOptions);

            if (message is null)
                throw new FeedCoreValidationException("Incoming message body was empty.");
        }
        catch (Exception exception) when (exception is JsonException or FeedCoreValidationException)
        {
            logger.LogWarning(
                "Rejected malformed normalized posting. Consumer={ConsumerNumber}, DeliveryTag={DeliveryTag}, Error={Error}",
                consumerNumber,
                eventArgs.DeliveryTag,
                exception.Message);

            await channel.BasicNackAsync(
                eventArgs.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken);
            return;
        }

        try
        {
            var posting = IncomingNormalizedPostingMapper.Map(message);

            using var scope = scopeFactory.CreateScope();
            var useCase = scope.ServiceProvider.GetRequiredService<AcceptNormalizedJobPostingUseCase>();
            var result = await useCase.ExecuteAsync(posting, cancellationToken);

            await channel.BasicAckAsync(
                eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken);

            logger.LogInformation(
                "Stored normalized posting. Consumer={ConsumerNumber}, JobPostingId={JobPostingId}, Created={Created}",
                consumerNumber,
                result.JobPostingId,
                result.Created);
        }
        catch (FeedCoreValidationException exception)
        {
            logger.LogWarning(
                "Rejected invalid normalized posting. Consumer={ConsumerNumber}, Source={Source}, Url={Url}, Error={Error}",
                consumerNumber,
                message.Source,
                message.Url,
                exception.Message);

            await channel.BasicNackAsync(
                eventArgs.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(
                exception,
                "Failed to persist normalized posting. Consumer={ConsumerNumber}, Source={Source}, Url={Url}",
                consumerNumber,
                message.Source,
                message.Url);

            await channel.BasicNackAsync(
                eventArgs.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken);
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

    private static ConnectionFactory CreateFactory(RabbitMqOptions options)
        => new()
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.Username,
            Password = options.Password,
            AutomaticRecoveryEnabled = true
        };
}
