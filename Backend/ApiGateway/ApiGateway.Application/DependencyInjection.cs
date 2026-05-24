using ApiGateway.Application.Interviews;
using ApiGateway.Application.Recommendations;
using ApiGateway.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace ApiGateway.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CurrentUserService>();
        services.AddScoped<UserInterviewService>();
        services.AddScoped<JobPostingLookupService>();
        services.AddScoped<LiveRecommendationService>();
        services.AddScoped<RecommendationSnapshotService>();
        services.AddScoped<StarterRecommendationInitializer>();
        return services;
    }
}
