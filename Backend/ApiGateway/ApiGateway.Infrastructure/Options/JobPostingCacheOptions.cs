using Microsoft.Extensions.Options;

namespace ApiGateway.Infrastructure.Options;

internal sealed class JobPostingCacheOptions
{
    public const string SectionName = "Recommendations";

    public TimeSpan JobPostingCacheTtl { get; init; } = TimeSpan.FromHours(6);
}

internal sealed class JobPostingCacheOptionsValidator : IValidateOptions<JobPostingCacheOptions>
{
    public ValidateOptionsResult Validate(string? name, JobPostingCacheOptions options)
        => options.JobPostingCacheTtl <= TimeSpan.Zero
            ? ValidateOptionsResult.Fail("Recommendations:JobPostingCacheTtl must be positive.")
            : ValidateOptionsResult.Success;
}
