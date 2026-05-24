using FeedCore.Application.Models;

namespace FeedCore.Application.Abstractions;

public interface IEmbeddingGenerator
{
    Task<EmbeddingVector> GenerateAsync(string text, CancellationToken cancellationToken);
}
