namespace AIServices.Models;

/// <summary>
/// Describes an embedding AI model exposed by the configured provider.
/// </summary>
public record EmbeddingAIModel
{
    /// <summary>
    /// Gets the application-level alias used to reference the embedding model.
    /// </summary>
    public required string ModelAlias { get; init; }

    /// <summary>
    /// Gets the provider-specific model name sent to the embedding service.
    /// </summary>
    public required string ModelName { get; init; }

    /// <summary>
    /// Gets an optional end-user identifier sent with embedding requests.
    /// </summary>
    public string? EndUserId { get; init; }

    /// <summary>
    /// Gets a value indicating whether the embedding model supports changing output dimensions.
    /// </summary>
    public bool SupportsDimensionControl { get; init; }

    /// <summary>
    /// Gets the requested output embedding dimension count when dimension control is supported.
    /// </summary>
    public int? DimensionCount { get; init; }
}
