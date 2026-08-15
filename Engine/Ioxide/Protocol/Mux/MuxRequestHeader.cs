using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Engine.Ioxide.Protocol.Mux;

/// <summary>
/// An <see cref="IRequestHeader"/> over the pseudo-headers a multiplexed protocol carries.
/// </summary>
internal sealed class MuxRequestHeader : IRequestHeader
{
    private static readonly ReadOnlyMemory<byte> HostName = "host"u8.ToArray();

    private readonly MuxKeyValueList _headers;

    private readonly MuxKeyValueList _query;

    private readonly RequestTarget _target;

    internal MuxRequestHeader(ReadOnlyMemory<byte> method, ReadOnlyMemory<byte> path, ReadOnlyMemory<byte> authority,
        List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> headers,
        List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> query, HttpProtocol protocol)
    {
        _headers = new MuxKeyValueList(WithHost(headers, authority));
        _query = new MuxKeyValueList(query);

        _target = new RequestTarget();

        // :path carries the query string, exactly as an HTTP/1.1 request target does. Routing must
        // see the path alone, or every request carrying a query 404s.
        Path = new ByteString(WithoutQuery(path));
        Method = new RequestMethod(method);

        Protocol = protocol;
        Version = protocol == HttpProtocol.Http3 ? Http3Version : Http2Version;

        _target.Apply(Path);
    }

    public RequestMethod Method { get; }

    public ByteString Path { get; }

    public IRequestTarget Target => _target;

    // Settled before a byte of the request arrived - by ALPN for HTTP/2, by QUIC plus ALPN for
    // HTTP/3 - so there is no version token on the wire to read.
    public HttpProtocol Protocol { get; }

    public ReadOnlyMemory<byte> Version { get; }

    public IRequestHeaders Headers => _headers;

    public IRequestQuery Query => _query;

    /// <summary>
    /// HTTP/2 and HTTP/3 carry the authority as the :authority pseudo-header, and clients omit Host
    /// entirely. RFC 9113 8.3.1 and RFC 9114 4.3.1 have an intermediary translating to HTTP/1.1
    /// construct Host from it, which is what this does: everything above the engine - routing,
    /// virtual hosting, redirects - expects a Host header to exist.
    /// </summary>
    private static List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> WithHost(
        List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> headers, ReadOnlyMemory<byte> authority)
    {
        if (authority.IsEmpty)
        {
            return headers;
        }

        foreach ((ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> _) in headers)
        {
            if (name.Length == 4 && Matches(name.Span, "host"u8))
            {
                return headers;
            }
        }

        var result = new List<(ReadOnlyMemory<byte>, ReadOnlyMemory<byte>)>(headers.Count + 1)
        {
            (HostName, authority),
        };

        result.AddRange(headers);

        return result;
    }

    private static bool Matches(ReadOnlySpan<byte> name, ReadOnlySpan<byte> lowercase)
    {
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];

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
        var mark = path.Span.IndexOf((byte)'?');

        return mark < 0 ? path : path[..mark];
    }

    private static readonly ReadOnlyMemory<byte> Http2Version = "HTTP/2.0"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> Http3Version = "HTTP/3.0"u8.ToArray();
}
