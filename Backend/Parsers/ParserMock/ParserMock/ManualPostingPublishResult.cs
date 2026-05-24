namespace ParserMock;

public sealed record ManualPostingPublishResult(
    string? Source,
    string? Url,
    string? Error)
{
    public static ManualPostingPublishResult Published(
        string source,
        string url)
    {
        return new ManualPostingPublishResult(source, url, Error: null);
    }

    public static ManualPostingPublishResult Failed(string? error)
    {
        return new ManualPostingPublishResult(Source: null, Url: null, error);
    }
}
