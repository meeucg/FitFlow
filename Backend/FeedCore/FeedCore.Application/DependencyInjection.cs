using FeedCore.Application.Options;
using FeedCore.Application.Rendering;
using FeedCore.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FeedCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFeedCoreApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IValidateOptions<RecommendationOptions>, RecommendationOptionsValidator>();
        services.AddSingleton<InterviewConclusionTextRenderer>();
        services.AddSingleton<JobPostingTextRenderer>();

        services.AddScoped<AddNewUserUseCase>();
        services.AddScoped<AcceptNormalizedJobPostingUseCase>();
        services.AddScoped<EmbedPendingJobPostingUseCase>();
        services.AddScoped<GetJobPostingUseCase>();
        services.AddScoped<RecoverPendingEmbeddingsUseCase>();

        return services;
    }
}
