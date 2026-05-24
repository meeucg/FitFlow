namespace ParserMock;

public sealed record RawPostingBatchRequest
{
    public IReadOnlyList<RawPostingRequest> Entries { get; init; } = [];
}
