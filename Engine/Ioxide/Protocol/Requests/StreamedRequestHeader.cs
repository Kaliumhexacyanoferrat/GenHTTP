using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Engine.Ioxide.Protocol.Requests;

/// <summary>The request head built from pseudo-headers, with :authority as Host and the query split off.</summary>
/// <remarks>
/// Built once per pooled request and refilled per stream: the two entry lists and the views over
/// them are the same objects every time, so a stream costs no allocation here.
/// </remarks>
internal sealed class StreamedRequestHeader : IRequestHeader
{
    private static readonly ReadOnlyMemory<byte> HostName = "host"u8.ToArray();

    private static readonly ByteString Host = new("host");

    private static readonly ReadOnlyMemory<byte> Http2Version = "HTTP/2.0"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> Http3Version = "HTTP/3.0"u8.ToArray();

    private readonly List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> _headerEntries;

    private readonly StreamedKeyValueList _headers;

    private readonly StreamedKeyValueList _query;

    private readonly RequestTarget _target = new();

    // The lists belong to the request, which refills them per stream; the views over them are built
    // once here and reused with it.
    internal StreamedRequestHeader(
        List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> headers,
        List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> query)
    {
        _headerEntries = headers;

        _headers = new StreamedKeyValueList(headers);
        _query = new StreamedKeyValueList(query);
    }

    public RequestMethod Method { get; private set; }

    public ByteString Path { get; private set; }

    public IRequestTarget Target => _target;

    public HttpProtocol Protocol { get; private set; }

    public ReadOnlyMemory<byte> Version { get; private set; }

    public IRequestHeaders Headers => _headers;

    public IRequestQuery Query => _query;

    // Settles the head once its headers are in: :authority is folded in as Host where the client
    // sent none.
    internal void Apply(ReadOnlyMemory<byte> method, ReadOnlyMemory<byte> path, ReadOnlyMemory<byte> authority, HttpProtocol protocol)
    {
        _headers.Prepend(HostFromAuthority(authority));

        Path = new ByteString(WithoutQuery(path));
        Method = new RequestMethod(method);

        Protocol = protocol;
        Version = protocol == HttpProtocol.Http3 ? Http3Version : Http2Version;

        _target.Apply(Path);
    }

    // Drops the folded-in Host; the request clears the lists themselves.
    internal void Reset() => _headers.Prepend(null);

    // :authority as a Host header to prepend, or null when there is nothing to add: the client sent
    // no authority, or a Host header of its own. Returned as an entry so the list is never copied.
    private (ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)? HostFromAuthority(ReadOnlyMemory<byte> authority)
    {
        if (authority.IsEmpty)
        {
            return null;
        }

        for (var i = 0; i < _headerEntries.Count; i++)
        {
            if (Host == _headerEntries[i].Name)
            {
                return null;
            }
        }

        return (HostName, authority);
    }

    // The path alone - routing must not see the query, or every request carrying one 404s.
    private static ReadOnlyMemory<byte> WithoutQuery(ReadOnlyMemory<byte> path)
    {
        var mark = path.Span.IndexOf((byte)'?');

        return mark < 0 ? path : path[..mark];
    }
}
