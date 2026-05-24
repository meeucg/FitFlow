using Microsoft.Extensions.Options;

namespace FeedCore.Persistence.Options;

public sealed class PersistenceOptions
{
    public const string ConnectionStringName = "Postgres";
}

public sealed class PersistenceOptionsValidator(string? connectionString) : IValidateOptions<PersistenceOptions>
{
    public ValidateOptionsResult Validate(string? name, PersistenceOptions options)
        => string.IsNullOrWhiteSpace(connectionString)
            ? ValidateOptionsResult.Fail("ConnectionStrings:Postgres is required.")
            : ValidateOptionsResult.Success;
}
