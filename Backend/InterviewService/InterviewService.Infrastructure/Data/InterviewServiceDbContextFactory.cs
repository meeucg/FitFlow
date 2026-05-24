using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InterviewService.Infrastructure.Data;

/// <summary>
/// Design-time DbContext factory used by EF Core tooling.
/// </summary>
public sealed class InterviewServiceDbContextFactory : IDesignTimeDbContextFactory<InterviewServiceDbContext>
{
    public InterviewServiceDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            ReadConnectionString("InterviewService.Api")
            ?? "Host=localhost;Port=55432;Database=fitflow_interviews;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<InterviewServiceDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new InterviewServiceDbContext(options);
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
