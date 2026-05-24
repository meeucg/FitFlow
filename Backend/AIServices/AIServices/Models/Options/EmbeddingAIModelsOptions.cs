namespace AIServices.Models.Options;

/// <summary>
/// Configures the default embedding AI model and any alternative embedding models available to the service.
/// </summary>
public record EmbeddingAIModelsOptions
{
    /// <summary>
    /// Gets the alternative embedding AI models that can be selected instead of the default model.
    /// </summary>
    public List<EmbeddingAIModel> AlternativeModels { get; init; } = [];

    /// <summary>
    /// Gets the embedding model used when no specific model is requested.
    /// </summary>
    public required EmbeddingAIModel DefaultModel { get; init; }
}
