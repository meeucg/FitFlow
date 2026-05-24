namespace AIServices.Models;

/// <summary>
/// Represents the result of an embedding AI operation.
/// </summary>
public record EmbeddingAIResponse
{
    /// <summary>
    /// Gets a value indicating whether the embedding operation completed successfully.
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the generated embedding vector.
    /// </summary>
    public required ReadOnlyMemory<float> Embedding { get; init; }

    /// <summary>
    /// Gets an unsuccessful response with no embedding vector.
    /// </summary>
    public static EmbeddingAIResponse NullResponse => new()
    {
        IsSuccess = false,
        Embedding = ReadOnlyMemory<float>.Empty,
    };
}
