namespace InterviewService.Infrastructure.Options;

/// <summary>
/// Configures inactivity threshold and sweep cadence for Redis-to-PostgreSQL archival.
/// </summary>
public sealed class InterviewArchivingOptions
{
    public const string SectionName = "Archiving";

    public TimeSpan InactiveAfter { get; set; }

    public TimeSpan SweepInterval { get; set; }

    public int BatchSize { get; set; } = 100;
}
