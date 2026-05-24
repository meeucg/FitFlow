using AIServices.Abstractions;
using FeedCore.Application.Abstractions;
using FeedCore.Application.Exceptions;
using FeedCore.Application.Models;

namespace FeedCore.Infrastructure.AI;

public sealed class AiServicesEmbeddingGenerator(IEmbeddingAI embeddingAI) : IEmbeddingGenerator
{
    public async Task<EmbeddingVector> GenerateAsync(string text, CancellationToken cancellationToken)
    {
        try
        {
            var response = await embeddingAI.EmbedText(text, cancellationToken);
            if (!response.IsSuccess || response.Embedding.IsEmpty)
                throw new EmbeddingProviderException("Embedding provider returned an unsuccessful response.");

            return new EmbeddingVector(response.Embedding.ToArray());
        }
        catch (EmbeddingProviderException)
        {
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new EmbeddingProviderException("Embedding provider call failed.", exception);
        }
    }
}
