using AIServices.Models;

namespace AIServices.Abstractions;

/// <summary>
/// Provides text embedding operations.
/// </summary>
public interface IEmbeddingAI
{
    /// <summary>
    /// Generates an embedding vector for the specified text.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <param name="ct">A token that can be used to cancel the operation.</param>
    /// <returns>The embedding operation result.</returns>
    Task<EmbeddingAIResponse> EmbedText(string text, CancellationToken ct = default);
}
