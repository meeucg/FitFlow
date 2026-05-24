using System.Text.Json.Serialization;
using FeedCore.Core.Models;

namespace FeedCore.Infrastructure.Messaging;

internal sealed record IncomingNormalizedJobPosting
{
    public required string Source { get; init; }
    public required DateTimeOffset PostedAt { get; init; }
    public required string Url { get; init; }
    public IncomingPostingPayload? Payload { get; init; }
}

internal sealed record IncomingPostingPayload
{
    public string? Author { get; init; }
    public string? Title { get; init; }
    public decimal? PriceMin { get; init; }
    public decimal? PriceMax { get; init; }
    public Currency Currency { get; init; } = Currency.Unspecified;
    public IReadOnlyList<IncomingPostingAttachment>? AttachedFiles { get; init; }
    public string? Description { get; init; }
    public string? Cluster { get; init; }
    public IReadOnlyList<IncomingJobPostingSpecialization> Specializations { get; init; } = [];
    public IReadOnlyList<IncomingJobPostingSkill> RequiredSkills { get; init; } = [];
    public IReadOnlyList<IncomingJobPostingSkill> BonusSkills { get; init; } = [];
    public IReadOnlyList<IncomingJobPostingTool> RequiredTools { get; init; } = [];
    public IReadOnlyList<IncomingJobPostingTool> BonusTools { get; init; } = [];
    public IReadOnlyList<IncomingJobPostingDomain> Domains { get; init; } = [];
}

internal sealed record IncomingPostingAttachment
{
    public string? Url { get; init; }
    [JsonPropertyName("base64")]
    public string? Base64 { get; init; }
    public required string Extension { get; init; }
}

internal sealed record IncomingJobPostingSkill
{
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public IReadOnlyList<string> AlternativeNames { get; init; } = [];
}

internal sealed record IncomingJobPostingTool
{
    public required string ToolStandardName { get; init; }
    public IReadOnlyList<string> ToolAltNames { get; init; } = [];
}

internal sealed record IncomingJobPostingSpecialization
{
    public required string Name { get; init; }
    public IReadOnlyList<string> AlternativeNames { get; init; } = [];
}

internal sealed record IncomingJobPostingDomain
{
    public required string Name { get; init; }
    public IReadOnlyList<string> AlternativeNames { get; init; } = [];
}
