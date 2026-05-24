using FeedCore.Application.Abstractions;
using FeedCore.Persistence.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FeedCore.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddFeedCorePersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(PersistenceOptions.ConnectionStringName);

        services.AddSingleton<IValidateOptions<PersistenceOptions>>(
            _ => new PersistenceOptionsValidator(connectionString));

        services.AddDbContext<FeedCoreDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<FeedCoreDatabaseMigrator>();
        services.AddScoped<IFeedCoreStore, FeedCoreStore>();

        return services;
    }
}
