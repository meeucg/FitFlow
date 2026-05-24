namespace FeedCore.Application.Models;

public sealed record InterviewConclusionData
{
    public string? Cluster { get; init; }
    public IReadOnlyList<ProfileSpecializationData> Specializations { get; init; } = [];
    public IReadOnlyList<ProfileSkillData> Skills { get; init; } = [];
    public IReadOnlyList<ProfileToolData> Tools { get; init; } = [];
    public IReadOnlyList<ProfileDomainData> PreferredDomains { get; init; } = [];
}

public sealed record ProfileSpecializationData
{
    public required string Name { get; init; }
    public IReadOnlyList<string> AlternativeNames { get; init; } = [];
}

public sealed record ProfileSkillData
{
    public required string DisplayName { get; init; }
    public string? Description { get; init; }
    public SkillDominanceLevel DominanceLevel { get; init; }
    public IReadOnlyList<string> AlternativeNames { get; init; } = [];
}

public sealed record ProfileToolData
{
    public required string ToolStandardName { get; init; }
    public ToolUsageFrequency UsageFrequency { get; init; }
    public IReadOnlyList<string> ToolAltNames { get; init; } = [];
}

public sealed record ProfileDomainData
{
    public required string Name { get; init; }
    public IReadOnlyList<string> AlternativeNames { get; init; } = [];
}

public enum SkillDominanceLevel
{
    Unspecified = 0,
    Core = 1,
    Important = 2,
    Secondary = 3,
    Limited = 4
}

public enum ToolUsageFrequency
{
    Unspecified = 0,
    Core = 1,
    Regular = 2,
    Occasional = 3,
    Rare = 4
}
