using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Engine.Ioxide.Protocol.Requests;

/// <summary>The request head built from pseudo-headers, with :authority as Host and the query split off.</summary>
internal sealed class StreamedRequestHeader : IRequestHeader
{
    private static readonly ReadOnlyMemory<byte> HostName = "host"u8.ToArray();

    private readonly StreamedKeyValueList _headers;

    private readonly StreamedKeyValueList _query;

    private readonly RequestTarget _target;

    // The request head, with :authority folded in as Host and the query taken off the path.
    internal StreamedRequestHeader(ReadOnlyMemory<byte> method, ReadOnlyMemory<byte> path, ReadOnlyMemory<byte> authority,
        List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> headers,
        List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> query, HttpProtocol protocol)
    {
        _headers = new StreamedKeyValueList(WithHost(headers, authority));
        _query = new StreamedKeyValueList(query);

        _target = new RequestTarget();

        Path = new ByteString(WithoutQuery(path));
        Method = new RequestMethod(method);

        Protocol = protocol;
        Version = protocol == HttpProtocol.Http3 ? Http3Version : Http2Version;

        _target.Apply(Path);
    }

    public RequestMethod Method { get; }

    public ByteString Path { get; }

    public IRequestTarget Target => _target;

    public HttpProtocol Protocol { get; }

    public ReadOnlyMemory<byte> Version { get; }

    public IRequestHeaders Headers => _headers;

    public IRequestQuery Query => _query;

    // Adds :authority as a Host header, unless the client sent one itself.
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

    // Compares a header name case-insensitively without allocating a string for it.
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

    // The path alone - routing must not see the query, or every request carrying one 404s.
    private static ReadOnlyMemory<byte> WithoutQuery(ReadOnlyMemory<byte> path)
    {
        var mark = path.Span.IndexOf((byte)'?');

        return mark < 0 ? path : path[..mark];
    }

    private static readonly ReadOnlyMemory<byte> Http2Version = "HTTP/2.0"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> Http3Version = "HTTP/3.0"u8.ToArray();
}
