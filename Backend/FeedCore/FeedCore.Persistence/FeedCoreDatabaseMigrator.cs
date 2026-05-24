using Microsoft.EntityFrameworkCore;

namespace FeedCore.Persistence;

public sealed class FeedCoreDatabaseMigrator(FeedCoreDbContext dbContext)
{
    public Task MigrateAsync(CancellationToken cancellationToken)
        => dbContext.Database.MigrateAsync(cancellationToken);
}
