using Microsoft.Extensions.Options;

namespace ApiGateway.Infrastructure.Options;

internal sealed class InterviewServiceOptions
{
    public const string SectionName = "InterviewService";

    public string? GrpcAddress { get; init; }
}

internal sealed class InterviewServiceOptionsValidator : IValidateOptions<InterviewServiceOptions>
{
    public ValidateOptionsResult Validate(string? name, InterviewServiceOptions options)
        => string.IsNullOrWhiteSpace(options.GrpcAddress)
            ? ValidateOptionsResult.Fail("InterviewService:GrpcAddress is required.")
            : ValidateOptionsResult.Success;
}
