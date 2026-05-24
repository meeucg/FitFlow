namespace AIServices.Models.Options;

/// <summary>
/// Configures the default text AI model and any alternative text models available to the service.
/// </summary>
public record TextAIModelsOptions
{
    /// <summary>
    /// Gets the alternative text AI models that can be selected instead of the default model.
    /// </summary>
    public List<TextAIModel> AlternativeModels { get; init; } = [];

    /// <summary>
    /// Gets the text model used when no specific model is requested.
    /// </summary>
    public required TextAIModel DefaultModel { get; init; }
}
