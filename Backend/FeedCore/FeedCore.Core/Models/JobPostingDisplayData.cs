namespace FeedCore.Core.Models;

public sealed record JobPostingDisplayData
{
    public required Guid Id { get; init; }
    public required string Source { get; init; }
    public required DateTimeOffset PostedAt { get; init; }
    public required string Url { get; init; }
    public string? Author { get; init; }
    public string? Title { get; init; }
    public decimal? PriceMin { get; init; }
    public decimal? PriceMax { get; init; }
    public Currency Currency { get; init; } = Currency.Unspecified;
    public string? Description { get; init; }
    public IReadOnlyList<PostingAttachmentData> AttachedFiles { get; init; } = [];
    public string? Cluster { get; init; }
    public IReadOnlyList<JobPostingSpecializationData> Specializations { get; init; } = [];
    public IReadOnlyList<JobPostingSkillData> RequiredSkills { get; init; } = [];
    public IReadOnlyList<JobPostingSkillData> BonusSkills { get; init; } = [];
    public IReadOnlyList<JobPostingToolData> RequiredTools { get; init; } = [];
    public IReadOnlyList<JobPostingToolData> BonusTools { get; init; } = [];
    public IReadOnlyList<JobPostingDomainData> Domains { get; init; } = [];
}

public sealed record PostingAttachmentData
{
    public string? Url { get; init; }
    public string? Base64 { get; init; }
    public required string Extension { get; init; }
}

public sealed record JobPostingSkillData
{
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public IReadOnlyList<string> AlternativeNames { get; init; } = [];
}

public sealed record JobPostingToolData
{
    public required string ToolStandardName { get; init; }
    public IReadOnlyList<string> ToolAltNames { get; init; } = [];
}

public sealed record JobPostingSpecializationData
{
    public required string Name { get; init; }
    public IReadOnlyList<string> AlternativeNames { get; init; } = [];
}

public sealed record JobPostingDomainData
{
    public required string Name { get; init; }
    public IReadOnlyList<string> AlternativeNames { get; init; } = [];
}
