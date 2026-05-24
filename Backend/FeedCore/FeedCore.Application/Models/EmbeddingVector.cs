namespace FeedCore.Application.Models;

public sealed record EmbeddingVector(float[] Values)
{
    public int Dimensions => Values.Length;
}
