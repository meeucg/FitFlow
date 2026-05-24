using System.Text.Json.Nodes;

namespace ParserMock;

public sealed record RawPostingRequest
{
    public string? Source { get; init; }

    public string? PostedAt { get; init; }

    public string? Url { get; init; }

    public JsonNode? Payload { get; init; }

    public string? RawText { get; init; }
}
