using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Shared.Types;

using Glyph3;

namespace GenHTTP.Engine.InternalH3Experimental.Protocol;

/// <summary>
/// An <see cref="IRequestHeader"/> over a Glyph3 request.
/// </summary>
internal sealed class H3RequestHeader : IRequestHeader
{
    private readonly H3KeyValueList _headers;

    private readonly H3KeyValueList _query;

    private readonly RequestTarget _target;

    internal H3RequestHeader(Http3Request source, List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> query)
    {
        _headers = new H3KeyValueList(WithHost(source));
        _query = new H3KeyValueList(query);

        _target = new RequestTarget();

        // :path carries the query string, exactly as an HTTP/1.1 request target does. Routing must
        // see the path alone, or every request with a query 404s.
        Path = new ByteString(WithoutQuery(source.Path));
        Method = new RequestMethod(source.Method);

        _target.Apply(Path);
    }

    public RequestMethod Method { get; }

    public ByteString Path { get; }

    public IRequestTarget Target => _target;

    // Always HTTP/3: there is no version on the wire to read. QUIC settles it, and ALPN said "h3"
    // before a single byte of this request arrived.
    public HttpProtocol Protocol => HttpProtocol.Http3;

    public ReadOnlyMemory<byte> Version => Http3Version;

    public IRequestHeaders Headers => _headers;

    public IRequestQuery Query => _query;

    /// <summary>
    /// HTTP/3 carries the authority as the :authority pseudo-header and clients omit Host entirely.
    /// RFC 9114 4.3.1 has an intermediary translating to HTTP/1.1 construct Host from it, which is
    /// what this does: everything above the engine expects a Host header to exist.
    /// </summary>
    private static List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> WithHost(Http3Request source)
    {
        if (source.Authority.IsEmpty)
        {
            return source.Headers;
        }

        foreach ((ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> _) in source.Headers)
        {
            if (name.Length == 4 && Matches(name.Span, "host"u8))
            {
                return source.Headers;
            }
        }

        var headers = new List<(ReadOnlyMemory<byte>, ReadOnlyMemory<byte>)>(source.Headers.Count + 1)
        {
            (HostName, source.Authority),
        };

        headers.AddRange(source.Headers);

        return headers;
    }

    private static bool Matches(ReadOnlySpan<byte> name, ReadOnlySpan<byte> lowercase)
    {
        for (int i = 0; i < name.Length; i++)
        {
            byte c = name[i];
            if (c is >= (byte)'A' and <= (byte)'Z')
            {
                c += 32;
            }
            if (c != lowercase[i])
            {
                return false;
            }
        }

        return true;
    }

    private static ReadOnlyMemory<byte> WithoutQuery(ReadOnlyMemory<byte> path)
    {
        int mark = path.Span.IndexOf((byte)'?');
        return mark < 0 ? path : path[..mark];
    }

    private static readonly ReadOnlyMemory<byte> HostName = "host"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> Http3Version = "HTTP/3.0"u8.ToArray();
}
