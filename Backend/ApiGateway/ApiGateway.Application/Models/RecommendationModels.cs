namespace ApiGateway.Application.Models;

/// <summary>
/// Recommendation SSE command envelope.
/// </summary>
/// <param name="CommandName">SSE command name.</param>
/// <param name="Data">Command payload.</param>
public sealed record RecommendationSseCommandDto(string CommandName, object Data);

/// <summary>
/// Batch recommendation payload.
/// </summary>
/// <param name="Ids">Recommended job posting ids.</param>
public sealed record RecommendationBatchDto(IReadOnlyList<string> Ids);

/// <summary>
/// Single recommendation payload.
/// </summary>
/// <param name="Id">Recommended job posting id.</param>
public sealed record RecommendationSingleDto(string Id);

/// <summary>
/// Job posting details returned by ApiGateway without embeddings.
/// </summary>
public sealed record JobPostingDto
{
    /// <summary>
    /// FeedCore job posting id.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Source system code.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Source posting timestamp.
    /// </summary>
    public required DateTimeOffset PostedAt { get; init; }

    /// <summary>
    /// Original posting URL.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Optional author display name.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Optional posting title.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Optional minimum budget.
    /// </summary>
    public string? PriceMin { get; init; }

    /// <summary>
    /// Optional maximum budget.
    /// </summary>
    public string? PriceMax { get; init; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Optional posting description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Optional professional cluster.
    /// </summary>
    public string? Cluster { get; init; }

    /// <summary>
    /// Attachments associated with the posting.
    /// </summary>
    public IReadOnlyList<PostingAttachmentDto> AttachedFiles { get; init; } = [];

    /// <summary>
    /// Posting specializations.
    /// </summary>
    public IReadOnlyList<NamedAliasesDto> Specializations { get; init; } = [];

    /// <summary>
    /// Required skills.
    /// </summary>
    public IReadOnlyList<JobPostingSkillDto> RequiredSkills { get; init; } = [];

    /// <summary>
    /// Bonus skills.
    /// </summary>
    public IReadOnlyList<JobPostingSkillDto> BonusSkills { get; init; } = [];

    /// <summary>
    /// Required tools.
    /// </summary>
    public IReadOnlyList<ToolAliasesDto> RequiredTools { get; init; } = [];

    /// <summary>
    /// Bonus tools.
    /// </summary>
    public IReadOnlyList<ToolAliasesDto> BonusTools { get; init; } = [];

    /// <summary>
    /// Business or product domains.
    /// </summary>
    public IReadOnlyList<NamedAliasesDto> Domains { get; init; } = [];
}

/// <summary>
/// Posting attachment DTO.
/// </summary>
public sealed record PostingAttachmentDto(string? Url, string? Base64, string Extension);

/// <summary>
/// DTO for named values with aliases.
/// </summary>
public sealed record NamedAliasesDto(string Name, IReadOnlyList<string> AlternativeNames);

/// <summary>
/// DTO for skill values.
/// </summary>
public sealed record JobPostingSkillDto(string DisplayName, string Description, IReadOnlyList<string> AlternativeNames);

/// <summary>
/// DTO for tool values.
/// </summary>
public sealed record ToolAliasesDto(string ToolStandardName, IReadOnlyList<string> ToolAltNames);
