using Microsoft.Extensions.Configuration;

namespace ParserMock;

public sealed record RabbitMqSettings
{
    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 5672;

    public string Username { get; init; } = "guest";

    public string Password { get; init; } = "guest";

    public string IncomingExchange { get; init; } = "raw-postings.incoming";

    public string IncomingQueue { get; init; } = "raw-postings-filter.incoming";

    public string IncomingRoutingKey { get; init; } = "job-posting.raw";

    public string DeadLetterExchange { get; init; } = "raw-postings.dead-letter";

    public string DeadLetterQueue { get; init; } = "raw-postings-filter.dead-letter";

    public TimeSpan PublishInterval { get; init; } = TimeSpan.FromSeconds(10);

    public string TelegramJsonlPath { get; init; } = "result.jsonl";

    public string KworkJsonPath { get; init; } = "kwork_result_with_extensions.json";

    public static RabbitMqSettings FromConfiguration(IConfiguration configuration)
    {
        var rabbitMq = configuration.GetSection("RabbitMq");
        var parserMock = configuration.GetSection("ParserMock");

        return new RabbitMqSettings
        {
            Host = Get(rabbitMq, nameof(Host), "localhost"),
            Port = GetInt(rabbitMq, nameof(Port), 5672),
            Username = Get(rabbitMq, nameof(Username), "guest"),
            Password = Get(rabbitMq, nameof(Password), "guest"),
            IncomingExchange = Get(rabbitMq, nameof(IncomingExchange), "raw-postings.incoming"),
            IncomingQueue = Get(rabbitMq, nameof(IncomingQueue), "raw-postings-filter.incoming"),
            IncomingRoutingKey = Get(rabbitMq, nameof(IncomingRoutingKey), "job-posting.raw"),
            DeadLetterExchange = Get(rabbitMq, nameof(DeadLetterExchange), "raw-postings.dead-letter"),
            DeadLetterQueue = Get(rabbitMq, nameof(DeadLetterQueue), "raw-postings-filter.dead-letter"),
            PublishInterval = TimeSpan.FromSeconds(GetInt(parserMock, "PublishIntervalSeconds", 10)),
            TelegramJsonlPath = Get(parserMock, nameof(TelegramJsonlPath), "result.jsonl"),
            KworkJsonPath = Get(parserMock, nameof(KworkJsonPath), "kwork_result_with_extensions.json")
        };
    }

    private static string Get(IConfiguration section, string name, string defaultValue)
    {
        return section[name] ?? defaultValue;
    }

    private static int GetInt(IConfiguration section, string name, int defaultValue)
    {
        var value = section[name];

        return int.TryParse(value, out var parsed) ? parsed : defaultValue;
    }
}
