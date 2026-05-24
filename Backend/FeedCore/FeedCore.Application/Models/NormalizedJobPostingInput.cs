using FeedCore.Core.Models;

namespace FeedCore.Application.Models;

public sealed record NormalizedJobPostingInput
{
    public required string Source { get; init; }
    public required DateTimeOffset PostedAt { get; init; }
    public required string Url { get; init; }
    public required JobPostingDisplayData DisplayData { get; init; }
}
