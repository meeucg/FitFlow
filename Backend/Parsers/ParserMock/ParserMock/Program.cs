using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using ParserMock;
using ParserMock.Observability;

var builder = WebApplication.CreateBuilder(args);
var settings = RabbitMqSettings.FromConfiguration(builder.Configuration);
var baseDirectory = AppContext.BaseDirectory;
var telegramJsonlPath = Path.Combine(baseDirectory, settings.TelegramJsonlPath);
var kworkJsonPath = Path.Combine(baseDirectory, settings.KworkJsonPath);
var postings = PostingPoolLoader.Load(telegramJsonlPath, kworkJsonPath);

if (postings.Count == 0)
{
    throw new InvalidOperationException("No parser mock postings were loaded.");
}

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
});

builder.Services.AddSingleton(settings);
builder.Services.AddSingleton(postings);
builder.Services.AddSingleton<RabbitMqRawPostingPublisher>();
builder.Services.AddHostedService<RandomPostingPublisherService>();
builder.Services.AddFitFlowObservability();

var app = builder.Build();

app.UseFitFlowObservability();

app.MapGet("/", () => Results.Ok(new
{
    service = "ParserMock",
    loaded_postings = postings.Count,
    publish_interval_seconds = settings.PublishInterval.TotalSeconds
}));

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost(
    "/raw-postings",
    async (
        RawPostingRequest request,
        RabbitMqRawPostingPublisher publisher,
        CancellationToken cancellationToken) =>
    {
        var result = await PublishManualPostingAsync(request, publisher, cancellationToken);

        return result.Error is null
            ? Results.Accepted(value: result)
            : Results.BadRequest(result);
    });

app.MapPost(
    "/raw-postings/batch",
    async (
        RawPostingBatchRequest request,
        RabbitMqRawPostingPublisher publisher,
        CancellationToken cancellationToken) =>
    {
        if (request.Entries.Count == 0)
        {
            return Results.BadRequest(new { error = "entries must contain at least one raw posting." });
        }

        var results = new List<ManualPostingPublishResult>(request.Entries.Count);

        foreach (var entry in request.Entries)
        {
            var result = await PublishManualPostingAsync(entry, publisher, cancellationToken);
            results.Add(result);

            if (result.Error is not null)
            {
                return Results.BadRequest(new
                {
                    error = "batch contains an invalid raw posting.",
                    results
                });
            }
        }

        return Results.Accepted(value: new
        {
            published_count = results.Count,
            postings = results
        });
    });

await app.RunAsync();

static async Task<ManualPostingPublishResult> PublishManualPostingAsync(
    RawPostingRequest request,
    RabbitMqRawPostingPublisher publisher,
    CancellationToken cancellationToken)
{
    if (!ManualRawPostingFactory.TryCreate(request, out var posting, out var error))
    {
        return ManualPostingPublishResult.Failed(error);
    }

    await publisher.PublishAsync(posting, cancellationToken);

    var source = posting["source"]?.GetValue<string>() ?? string.Empty;
    var url = posting["url"]?.GetValue<string>() ?? string.Empty;

    Console.WriteLine($"{DateTimeOffset.UtcNow:O} manually published {source} {url}");

    return ManualPostingPublishResult.Published(source, url);
}
