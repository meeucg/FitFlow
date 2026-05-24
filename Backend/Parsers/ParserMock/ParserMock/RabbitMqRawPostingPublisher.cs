using System.Text;
using System.Text.Json.Nodes;
using RabbitMQ.Client;

namespace ParserMock;

public sealed class RabbitMqRawPostingPublisher(
    RabbitMqSettings settings) : IAsyncDisposable
{
    private readonly SemaphoreSlim publishLock = new(1, 1);
    private IConnection? connection;
    private IChannel? channel;

    public async Task PublishAsync(
        JsonObject posting,
        CancellationToken cancellationToken)
    {
        await publishLock.WaitAsync(cancellationToken);

        try
        {
            await EnsureConnectedAsync(cancellationToken);

            var body = Encoding.UTF8.GetBytes(PostingPoolLoader.Serialize(posting));

            await channel!.BasicPublishAsync(
                exchange: settings.IncomingExchange,
                routingKey: settings.IncomingRoutingKey,
                mandatory: false,
                basicProperties: CreateJsonProperties(),
                body: body,
                cancellationToken: cancellationToken);
        }
        finally
        {
            publishLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (channel is not null)
        {
            await channel.DisposeAsync();
        }

        if (connection is not null)
        {
            await connection.DisposeAsync();
        }

        publishLock.Dispose();
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (channel is not null && channel.IsOpen)
        {
            return;
        }

        if (channel is not null)
        {
            await channel.DisposeAsync();
            channel = null;
        }

        if (connection is not null && !connection.IsOpen)
        {
            await connection.DisposeAsync();
            connection = null;
        }

        connection ??= await ConnectWithRetryAsync(cancellationToken);
        channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await DeclareTopologyAsync(channel, cancellationToken);
    }

    private async Task<IConnection> ConnectWithRetryAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            UserName = settings.Username,
            Password = settings.Password,
            AutomaticRecoveryEnabled = true
        };

        while (true)
        {
            try
            {
                return await factory.CreateConnectionAsync(cancellationToken);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine(
                    $"{DateTimeOffset.UtcNow:O} RabbitMQ connection failed: {exception.Message}. Retrying in 5s.");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }

    private async Task DeclareTopologyAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            settings.IncomingExchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            settings.DeadLetterExchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            settings.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            settings.DeadLetterQueue,
            settings.DeadLetterExchange,
            settings.IncomingRoutingKey,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            settings.IncomingQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = settings.DeadLetterExchange,
                ["x-dead-letter-routing-key"] = settings.IncomingRoutingKey
            },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            settings.IncomingQueue,
            settings.IncomingExchange,
            settings.IncomingRoutingKey,
            cancellationToken: cancellationToken);
    }

    private static BasicProperties CreateJsonProperties()
    {
        return new BasicProperties
        {
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            Persistent = true,
            MessageId = Guid.NewGuid().ToString("N"),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };
    }
}
