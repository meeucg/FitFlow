using ApiGateway.Application.Recommendations;
using ApiGateway.Options;
using Microsoft.Extensions.Options;

namespace ApiGateway.Services;

/// <summary>
/// Initializes starter recommendations for users after the interview conclusion is available.
/// </summary>
public sealed class RecommendationInitializerHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<RecommendationsOptions> options,
    ILogger<RecommendationInitializerHostedService> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var initializer = scope.ServiceProvider.GetRequiredService<StarterRecommendationInitializer>();
                var results = await initializer.ProcessBatchAsync(
                    new StarterRecommendationInitializationSettings(
                        options.Value.InitializationRetryDelay,
                        options.Value.InitializationMaxRetries,
                        BatchSize: 10),
                    stoppingToken);

                foreach (var result in results.Where(x => x.Succeeded))
                {
                    logger.LogInformation(
                        "Initialized starter recommendations. UserId={UserId}, Count={Count}",
                        result.UserId,
                        result.RecommendationCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Starter recommendation initializer failed its current batch.");
            }

            await Task.Delay(options.Value.InitializationPollInterval, stoppingToken);
        }
    }
}
