using AIServices.ServiceBuilders;
using FeedCore.Application.Abstractions;
using FeedCore.Infrastructure.AI;
using FeedCore.Infrastructure.HostedServices;
using FeedCore.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FeedCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFeedCoreInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<RabbitMqOptions>, RabbitMqOptionsValidator>();

        services.AddAIServices(
            configuration.GetSection("TextAI"),
            configuration.GetSection("TextAIModels"),
            configuration.GetSection("EmbeddingAI"),
            configuration.GetSection("EmbeddingAIModels"));

        services.AddScoped<IEmbeddingGenerator, AiServicesEmbeddingGenerator>();
        services.AddHostedService<NormalizedPostingConsumerHostedService>();
        services.AddHostedService<PendingEmbeddingHostedService>();
        services.AddHostedService<OutboxPublisherHostedService>();

        return services;
    }
}
