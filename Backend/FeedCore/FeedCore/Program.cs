using FeedCore.Application;
using FeedCore.Application.Options;
using FeedCore.Infrastructure;
using FeedCore.Observability;
using FeedCore.Persistence;
using FeedCore.Persistence.Options;
using FeedCore.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddFitFlowObservability();

builder.Services.AddOptions<RecommendationOptions>()
    .Bind(builder.Configuration.GetSection(RecommendationOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddOptions<PersistenceOptions>()
    .ValidateOnStart();

builder.Services
    .AddFeedCoreApplication()
    .AddFeedCorePersistence(builder.Configuration)
    .AddFeedCoreInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var migrator = scope.ServiceProvider.GetRequiredService<FeedCoreDatabaseMigrator>();
    await migrator.MigrateAsync(CancellationToken.None);
}

app.MapGrpcService<FeedCoreGatewayService>();
app.UseFitFlowObservability();
app.MapGet("/", () => "FitFlow FeedCore recommendation service is running.");

app.Run();
