using Microsoft.Extensions.Options;

namespace ApiGateway.Infrastructure.Options;

/// <summary>
/// FeedCore gRPC client options.
/// </summary>
public sealed class FeedCoreOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "FeedCore";

    /// <summary>
    /// FeedCore gRPC endpoint address.
    /// </summary>
    public string? GrpcAddress { get; init; }
}

/// <summary>
/// Validates FeedCore client configuration.
/// </summary>
public sealed class FeedCoreOptionsValidator : IValidateOptions<FeedCoreOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, FeedCoreOptions options)
        => string.IsNullOrWhiteSpace(options.GrpcAddress)
            ? ValidateOptionsResult.Fail("FeedCore:GrpcAddress is required.")
            : ValidateOptionsResult.Success;
}
