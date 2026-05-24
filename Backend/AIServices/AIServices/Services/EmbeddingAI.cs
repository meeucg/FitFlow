using System.ClientModel;
using AIServices.Abstractions;
using AIServices.Models;
using AIServices.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;

namespace AIServices.Services;

public class EmbeddingAI : IEmbeddingAI
{
    private readonly EmbeddingAIModel _model;
    private readonly EmbeddingAIOptions _embeddingAIOptions;
    private readonly ILogger<EmbeddingAI>? _logger;
    private readonly EmbeddingClient _client;

    public EmbeddingAI(
        IOptions<EmbeddingAIOptions> opt,
        EmbeddingAIModel model,
        ILogger<EmbeddingAI>? logger = null)
    {
        _model = model;
        _embeddingAIOptions = opt.Value;
        _logger = logger;

        _client = new EmbeddingClient(
            _model.ModelName,
            new ApiKeyCredential(_embeddingAIOptions.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(_embeddingAIOptions.ApiEndpoint)
            });
    }

    public async Task<EmbeddingAIResponse> EmbedText(string text, CancellationToken ct = default)
    {
        var options = new EmbeddingGenerationOptions
        {
            EndUserId = _model.EndUserId,
            Dimensions = _model.SupportsDimensionControl
                ? _model.DimensionCount
                : null
        };

        for (var i = 0; i < _embeddingAIOptions.RetryCount; i++)
        {
            using var timeoutCts = new CancellationTokenSource(_embeddingAIOptions.RetryAfter);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                ct,
                timeoutCts.Token);

            timeoutCts.Token.Register(_ =>
            {
                _logger?.LogWarning("Embedding request timed out");
            }, null);

            try
            {
                ClientResult<OpenAIEmbedding> result = await _client.GenerateEmbeddingAsync(
                    text,
                    options,
                    linkedCts.Token);

                return new EmbeddingAIResponse
                {
                    IsSuccess = true,
                    Embedding = result.Value.ToFloats()
                };
            }
            catch (OperationCanceledException)
            {
                if (timeoutCts.IsCancellationRequested && !ct.IsCancellationRequested)
                    continue;

                _logger?.LogInformation("Embedding request was canceled");
                return EmbeddingAIResponse.NullResponse;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(
                    "Embedding request failed at attempt number {iteration} with Exception message:\n {ex},\n Stack trace:\n {trace}",
                    i,
                    ex.Message,
                    ex.StackTrace);
            }
        }

        _logger?.LogError("Embedding request failed at all attempts, returning null response");
        return EmbeddingAIResponse.NullResponse;
    }
}
