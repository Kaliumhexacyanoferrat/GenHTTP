using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Engine.Ioxide.Protocol.Multiplexed;

internal sealed class MultiplexedRequestHeader : IRequestHeader
{
    private static readonly ReadOnlyMemory<byte> HostName = "host"u8.ToArray();

    private readonly MultiplexedKeyValueList _headers;

    private readonly MultiplexedKeyValueList _query;

    private readonly RequestTarget _target;

    internal MultiplexedRequestHeader(ReadOnlyMemory<byte> method, ReadOnlyMemory<byte> path, ReadOnlyMemory<byte> authority,
        List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> headers,
        List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> query, HttpProtocol protocol)
    {
        _headers = new MultiplexedKeyValueList(WithHost(headers, authority));
        _query = new MultiplexedKeyValueList(query);

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
