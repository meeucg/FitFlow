using ApiGateway.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiGateway.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApiGatewayDbContext>(
            options => options.UseNpgsql(configuration.GetConnectionString("ApiGateway")));
        services.AddScoped<IUnitOfWork, ApiGatewayUnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJobRecommendationRepository, JobRecommendationRepository>();

        return services;
    }
}
