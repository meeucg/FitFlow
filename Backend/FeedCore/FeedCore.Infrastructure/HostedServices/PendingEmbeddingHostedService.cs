using FeedCore.Application.Options;
using FeedCore.Application.UseCases;
using FeedCore.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeedCore.Infrastructure.HostedServices;

public sealed class PendingEmbeddingHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<RecommendationOptions> recommendationOptions,
    IOptions<RabbitMqOptions> rabbitMqOptions,
    ILogger<PendingEmbeddingHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RecoverProcessingJobsAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var useCase = scope.ServiceProvider.GetRequiredService<EmbedPendingJobPostingUseCase>();
                var result = await useCase.ExecuteBatchAsync(
                    rabbitMqOptions.Value.OutgoingRecommendationsExchange,
                    rabbitMqOptions.Value.OutgoingRecommendationsRoutingKey,
                    stoppingToken);

                if (result.Claimed > 0)
                {
                    logger.LogInformation(
                        "Processed pending job embedding batch. Claimed={Claimed}, Embedded={Embedded}, Failed={Failed}, RecommendationMessages={RecommendationMessages}",
                        result.Claimed,
                        result.Embedded,
                        result.Failed,
                        result.RecommendationsCreated);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Pending job embedding worker failed its current batch.");
            }

            await Task.Delay(recommendationOptions.Value.PendingEmbeddingPollInterval, stoppingToken);
        }
    }

    private async Task RecoverProcessingJobsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var useCase = scope.ServiceProvider.GetRequiredService<RecoverPendingEmbeddingsUseCase>();
            var recovered = await useCase.ExecuteAsync(cancellationToken);

            if (recovered > 0)
                logger.LogInformation("Recovered interrupted job embedding attempts. Count={Recovered}", recovered);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Failed to recover interrupted job embedding attempts.");
        }
    }
}
