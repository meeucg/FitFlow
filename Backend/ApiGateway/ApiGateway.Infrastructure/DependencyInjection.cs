using ApiGateway.Application.Abstractions;
using ApiGateway.Infrastructure.ExternalServices;
using ApiGateway.Infrastructure.Mapping;
using ApiGateway.Infrastructure.Options;
using FeedCoreContracts = FitFlow.FeedCore.Grpc.Contracts;
using FitFlow.Interview.Grpc.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ApiGateway.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<FeedCoreOptions>()
            .Bind(configuration.GetSection(FeedCoreOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<FeedCoreOptions>, FeedCoreOptionsValidator>();

        services.AddOptions<InterviewServiceOptions>()
            .Bind(configuration.GetSection(InterviewServiceOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<InterviewServiceOptions>, InterviewServiceOptionsValidator>();

        services.AddOptions<JobPostingCacheOptions>()
            .Bind(configuration.GetSection(JobPostingCacheOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<JobPostingCacheOptions>, JobPostingCacheOptionsValidator>();

        services.AddAutoMapper(_ => { }, typeof(InterviewMappingProfile).Assembly);
        services.AddMemoryCache();

        services.AddGrpcClient<InterviewGateway.InterviewGatewayClient>(
            (serviceProvider, options) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<InterviewServiceOptions>>().Value;
                options.Address = new Uri(settings.GrpcAddress!);
            });

        services.AddGrpcClient<FeedCoreContracts.FeedCoreGateway.FeedCoreGatewayClient>(
            (serviceProvider, options) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<FeedCoreOptions>>().Value;
                options.Address = new Uri(settings.GrpcAddress!);
            });

        services.AddScoped<IInterviewGateway, InterviewGrpcGateway>();
        services.AddScoped<IFeedCoreGateway, FeedCoreGrpcGateway>();
        return services;
    }
}
