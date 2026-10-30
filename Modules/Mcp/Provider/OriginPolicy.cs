namespace GenHTTP.Modules.Mcp.Provider;

/// <summary>
/// Decides which web pages may use the endpoint from a browser.
/// </summary>
/// <remarks>
/// Agents do not send an origin and are always accepted. A browser always does, and
/// without this check a page of any site could drive the endpoint, including via
/// DNS rebinding, which is why comparing the origin with the host does not suffice.
/// </remarks>
internal sealed class OriginPolicy
{
    private readonly HashSet<string> _allowed;

    internal OriginPolicy(IEnumerable<string> allowed)
    {
        _allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var origin in allowed)
        {
            _allowed.Add(Normalize(origin) ?? throw new ArgumentException($"'{origin}' is not a valid origin (such as 'https://example.com').", nameof(allowed)));
        }
    }

    internal bool Allows(string origin)
    {
        var normalized = Normalize(origin);

        return normalized != null && _allowed.Contains(normalized);
    }

    private static string? Normalize(string origin)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        return $"{uri.Scheme}://{uri.Host}:{uri.Port}";
    }

}
