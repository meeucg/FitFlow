namespace ApiGateway.Application.Models;

/// <summary>
/// Describes an API error returned by ApiGateway.
/// </summary>
/// <param name="Message">Human-readable error details suitable for diagnostics and UI display.</param>
public sealed record ErrorDto(string Message);

/// <summary>
/// Full interview display used internally before ApiGateway removes the service-owned interview id.
/// </summary>
public sealed record InterviewDisplayDto
{
    /// <summary>
    /// InterviewService interview identifier; never exposed by user-owned REST endpoints.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Static setup metadata and required setup questions for the interview.
    /// </summary>
    public InterviewSetupDto? Setup { get; init; }

    /// <summary>
    /// Answers already submitted for the setup questions.
    /// </summary>
    public IReadOnlyList<AnswerDto> RequiredAnswers { get; init; } = [];

    /// <summary>
    /// Transcript of completed question and answer pairs.
    /// </summary>
    public IReadOnlyList<InterviewStepDto> CompletedSteps { get; init; } = [];

    /// <summary>
    /// Current question the user should answer next, or null when the interview has concluded.
    /// </summary>
    public QuestionDto? CurrentQuestion { get; init; }

    /// <summary>
    /// Final generated user profile when the interview has concluded.
    /// </summary>
    public UserProfileDto? Conclusion { get; init; }
}

/// <summary>
/// User-facing interview display returned by <c>GET /my-interview</c> without exposing the interview id.
/// </summary>
public sealed record MyInterviewDisplayDto
{
    /// <summary>
    /// Static setup metadata and required setup questions for the interview.
    /// </summary>
    public InterviewSetupDto? Setup { get; init; }

    /// <summary>
    /// Answers already submitted for the setup questions.
    /// </summary>
    public IReadOnlyList<AnswerDto> RequiredAnswers { get; init; } = [];

    /// <summary>
    /// Transcript of completed question and answer pairs.
    /// </summary>
    public IReadOnlyList<InterviewStepDto> CompletedSteps { get; init; } = [];

    /// <summary>
    /// Current question the user should answer next, or null when the interview has concluded.
    /// </summary>
    public QuestionDto? CurrentQuestion { get; init; }

    /// <summary>
    /// Final generated user profile when the interview has concluded.
    /// </summary>
    public UserProfileDto? Conclusion { get; init; }
}

/// <summary>
/// Immutable setup definition used to initialize an interview.
/// </summary>
public sealed record InterviewSetupDto
{
    /// <summary>
    /// Deterministic setup hash GUID produced from the setup group and payload.
    /// </summary>
    public required string HashGuid { get; init; }

    /// <summary>
    /// Required questions that must be answered before dynamic interview generation starts.
    /// </summary>
    public IReadOnlyList<QuestionDto> RequiredQuestions { get; init; } = [];
}

/// <summary>
/// Completed interview step containing a question and the answer submitted for it.
/// </summary>
public sealed record InterviewStepDto
{
    /// <summary>
    /// Question shown to the user for this step.
    /// </summary>
    public QuestionDto? Question { get; init; }

    /// <summary>
    /// Answer submitted by the user for this step.
    /// </summary>
    public AnswerDto? Answer { get; init; }
}

/// <summary>
/// Next piece of interview UI returned after an answer is submitted.
/// </summary>
public sealed record FormElementDto
{
    /// <summary>
    /// Next question to ask, or null when the interview has produced a final profile.
    /// </summary>
    public QuestionDto? Question { get; init; }

    /// <summary>
    /// Final generated user profile, or null while more questions remain.
    /// </summary>
    public UserProfileDto? UserProfile { get; init; }
}

/// <summary>
/// Interview question with choice, level, text, and optional-skip metadata.
/// </summary>
public sealed record QuestionDto
{
    /// <summary>
    /// Human-readable question text shown to the user.
    /// </summary>
    public required string QuestionText { get; init; }

    /// <summary>
    /// Available zero-based answer options, excluding any free-text option.
    /// </summary>
    public IReadOnlyList<string> AnswerOptions { get; init; } = [];

    /// <summary>
    /// Optional zero-based level labels that can be attached to selected options.
    /// </summary>
    public IReadOnlyList<string> AnswerLevels { get; init; } = [];

    /// <summary>
    /// Indicates whether the user may submit a custom free-text answer.
    /// </summary>
    public bool PlainTextOptionPresent { get; init; }

    /// <summary>
    /// Indicates whether the user should select exactly one answer option.
    /// </summary>
    public bool IsSingleChoice { get; init; }

    /// <summary>
    /// Indicates whether the question may be skipped.
    /// </summary>
    public bool IsOptional { get; init; }
}

/// <summary>
/// User answer submitted to the current interview question.
/// </summary>
public sealed record AnswerDto
{
    /// <summary>
    /// Selected option indexes and optional selected levels.
    /// </summary>
    public IReadOnlyList<OptionAnswerDto> SelectedOptions { get; init; } = [];

    /// <summary>
    /// Optional free-text answer when the question supports text input.
    /// </summary>
    public string? TextAnswer { get; init; }

    /// <summary>
    /// Indicates that an optional question was intentionally skipped.
    /// </summary>
    public bool IsSkipped { get; init; }
}

/// <summary>
/// Selected answer option and optional level for a question.
/// </summary>
public sealed record OptionAnswerDto
{
    /// <summary>
    /// Zero-based index into the question's <see cref="QuestionDto.AnswerOptions"/> collection.
    /// </summary>
    public int OptionId { get; init; }

    /// <summary>
    /// Optional zero-based index into the question's <see cref="QuestionDto.AnswerLevels"/> collection.
    /// </summary>
    public int? SelectedLevel { get; init; }
}

/// <summary>
/// Generated professional profile produced when the interview concludes.
/// </summary>
public sealed record UserProfileDto
{
    /// <summary>
    /// Broad professional cluster inferred for the user.
    /// </summary>
    public required string Cluster { get; init; }

    /// <summary>
    /// Specializations inferred from the interview answers.
    /// </summary>
    public IReadOnlyList<SpecializationDto> Specializations { get; init; } = [];

    /// <summary>
    /// Skills inferred from the interview answers.
    /// </summary>
    public IReadOnlyList<SkillDto> Skills { get; init; } = [];

    /// <summary>
    /// Tools inferred from the interview answers.
    /// </summary>
    public IReadOnlyList<ToolDto> Tools { get; init; } = [];

    /// <summary>
    /// Preferred work domains inferred from the interview answers.
    /// </summary>
    public IReadOnlyList<DomainDto> PreferredDomains { get; init; } = [];
}

/// <summary>
/// Professional specialization with optional alternative names.
/// </summary>
public sealed record SpecializationDto
{
    /// <summary>
    /// Canonical specialization name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Alternative names or aliases for the specialization.
    /// </summary>
    public IReadOnlyList<string> AlternativeNames { get; init; } = [];
}

/// <summary>
/// Skill inferred for the user profile.
/// </summary>
public sealed record SkillDto
{
    /// <summary>
    /// User-facing skill name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Short description of the skill's meaning in the profile.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Relative importance of the skill, returned as core, important, secondary, limited, or unspecified.
    /// </summary>
    public required string DominanceLevel { get; init; }

    /// <summary>
    /// Alternative names or aliases for the skill.
    /// </summary>
    public IReadOnlyList<string> AlternativeNames { get; init; } = [];
}

/// <summary>
/// Tool inferred for the user profile.
/// </summary>
public sealed record ToolDto
{
    /// <summary>
    /// Canonical tool name.
    /// </summary>
    public required string ToolStandardName { get; init; }

    /// <summary>
    /// Relative tool usage frequency, returned as core, regular, occasional, rare, or unspecified.
    /// </summary>
    public required string UsageFrequency { get; init; }

    /// <summary>
    /// Alternative names or aliases for the tool.
    /// </summary>
    public IReadOnlyList<string> ToolAltNames { get; init; } = [];
}

/// <summary>
/// Preferred professional domain inferred for the user profile.
/// </summary>
public sealed record DomainDto
{
    /// <summary>
    /// Canonical domain name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Alternative names or aliases for the domain.
    /// </summary>
    public IReadOnlyList<string> AlternativeNames { get; init; } = [];
}
