using Microsoft.Extensions.Options;

namespace ApiGateway.Options;

internal sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string? Authority { get; init; }

    public string? MetadataAddress { get; init; }

    public string? BackchannelAuthority { get; init; }

    public string? Audience { get; init; }

    public bool RequireHttpsMetadata { get; init; } = true;
}

internal sealed class AuthenticationOptionsValidator : IValidateOptions<AuthenticationOptions>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Authority))
            return ValidateOptionsResult.Fail("Authentication:Authority is required.");

        if (string.IsNullOrWhiteSpace(options.MetadataAddress))
            return ValidateOptionsResult.Fail("Authentication:MetadataAddress is required.");

        if (string.IsNullOrWhiteSpace(options.Audience))
            return ValidateOptionsResult.Fail("Authentication:Audience is required.");

        return ValidateOptionsResult.Success;
    }
}
