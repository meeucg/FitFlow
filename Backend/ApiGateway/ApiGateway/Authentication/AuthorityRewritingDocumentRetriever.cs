using Microsoft.IdentityModel.Protocols;

namespace ApiGateway.Authentication;

internal sealed class AuthorityRewritingDocumentRetriever(
    string publicAuthority,
    string? backchannelAuthority,
    bool requireHttps) : IDocumentRetriever
{
    private readonly string publicAuthority = TrimSlash(publicAuthority);
    private readonly string? backchannelAuthority = string.IsNullOrWhiteSpace(backchannelAuthority)
        ? null
        : TrimSlash(backchannelAuthority);
    private readonly HttpDocumentRetriever inner = new() { RequireHttps = requireHttps };

    public Task<string> GetDocumentAsync(string address, CancellationToken cancel)
        => inner.GetDocumentAsync(Rewrite(address), cancel);

    private string Rewrite(string address)
        => backchannelAuthority is not null &&
           address.StartsWith(publicAuthority, StringComparison.OrdinalIgnoreCase)
            ? backchannelAuthority + address[publicAuthority.Length..]
            : address;

    private static string TrimSlash(string value)
        => value.TrimEnd('/');
}
