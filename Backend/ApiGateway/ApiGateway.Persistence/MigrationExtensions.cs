using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApiGateway.Persistence;

public static class MigrationExtensions
{
    public static async Task MigrateApiGatewayDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApiGatewayDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
