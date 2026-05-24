using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FeedCore.Persistence;

public sealed class FeedCoreDbContextDesignTimeFactory : IDesignTimeDbContextFactory<FeedCoreDbContext>
{
    public FeedCoreDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            ReadConnectionString("FeedCore")
            ?? "Host=localhost;Port=55434;Database=fitflow_feedcore;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<FeedCoreDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new FeedCoreDbContext(options);
    }

    private static string? ReadConnectionString(string startupProjectName)
    {
        foreach (var path in EnumerateAppSettingsFiles(startupProjectName))
        {
            if (!File.Exists(path))
            {
                continue;
            }

            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings)
                && connectionStrings.TryGetProperty("Postgres", out var postgres)
                && postgres.GetString() is { Length: > 0 } connectionString)
            {
                return connectionString;
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateAppSettingsFiles(string startupProjectName)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var candidateDirectories = new[]
        {
            currentDirectory,
            Path.Combine(currentDirectory, startupProjectName),
            Path.Combine(currentDirectory, "..", startupProjectName)
        };

        foreach (var directory in candidateDirectories)
        {
            yield return Path.GetFullPath(Path.Combine(directory, "appsettings.Development.json"));
            yield return Path.GetFullPath(Path.Combine(directory, "appsettings.json"));
        }
    }
}
