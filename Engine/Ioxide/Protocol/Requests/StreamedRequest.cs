using System.Buffers.Text;
using System.IO.Pipelines;
using System.Net;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Engine.Ioxide.Protocol.Requests;

/// <summary>One HTTP/2 or HTTP/3 stream, as the request shape the handler chain expects.</summary>
/// <remarks>
/// Pooled per reactor and reused stream after stream, the way Http1Driver reuses its Request. A
/// stream is short-lived and a multiplexed connection carries many at once, so allocating this
/// graph per stream is what the pool exists to avoid. It also carries the scratch the response side
/// needs - the header list and the content-length buffer - so a whole exchange allocates nothing.
/// </remarks>
internal sealed class StreamedRequest : IRequest
{
    [ThreadStatic]
    private static Stack<StreamedRequest>? _pool;

    private const int MaxPooled = 1024;

    private static readonly ReadOnlyMemory<byte> ContentLength = "content-length"u8.ToArray();

    // Owned here and refilled per stream; the header holds views over them.
    private readonly List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> _headerEntries = [];

    private readonly List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> _queryEntries = [];

    private readonly StreamedRequestHeader _header;

    private readonly StreamedRequestBody _body = new();

    private readonly ClientConnection _client = new();

    private readonly PropertyBag _properties = new();

    private readonly ResponseBuilder _response = new();

    // ulong tops out at 20 digits, so one buffer serves every content length.
    private readonly byte[] _digits = new byte[20];

    private IServer? _server;

    private IEndPoint? _endPoint;

    private bool _hasBody;

    private Func<IRequestBody, IRequestBody>? _bodyWrapper;

    private bool _bodyFetched;

    /// <summary>The response head being built for this stream, reused with the request.</summary>
    internal List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> ResponseHeaders { get; } = [];

    private StreamedRequest()
    {
        _header = new StreamedRequestHeader(_headerEntries, _queryEntries);
    }

    // One off this reactor's pool, or a new one. Its lists are already empty.
    internal static StreamedRequest Rent() => _pool is { } pool && pool.TryPop(out var request) ? request : new StreamedRequest();

    // Back to the pool, emptied, up to the ceiling.
    internal static void Return(StreamedRequest request)
    {
        request.Reset();

        var pool = _pool ??= new Stack<StreamedRequest>();

        if (pool.Count < MaxPooled)
        {
            pool.Push(request);
        }
    }

    // One header off the wire, added before Apply so the head can fold in :authority.
    internal void AddHeader(ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> value) => _headerEntries.Add((name, value));

    // Points this request at one stream, once its headers are in.
    internal void Apply(IServer server, IEndPoint endPoint, ReadOnlyMemory<byte> method, ReadOnlyMemory<byte> path,
        ReadOnlyMemory<byte> authority, Func<ValueTask<ReadOnlyMemory<byte>>>? read, IPAddress? remoteAddress,
        HttpProtocol protocol, bool secure)
    {
        _server = server;
        _endPoint = endPoint;

        ParseQuery(path, _queryEntries);

        _header.Apply(method, path, authority, protocol);

        // A stream carries a body only when it declares one: HTTP/2 and HTTP/3 forbid Transfer-Encoding,
        // so Content-Length is the signal - and its absence means no body, matching what the other engines
        // report for a request that carries neither header.
        _hasBody = read is not null && HasBody(_headerEntries);

        if (_hasBody)
        {
            _body.Apply(read!);
        }

        _client.Apply(remoteAddress, secure ? ClientProtocol.Https : ClientProtocol.Http, null);
    }

    // A content length as ASCII digits, into the buffer this exchange already owns.
    internal ReadOnlyMemory<byte> Digits(ulong value)
    {
        Utf8Formatter.TryFormat(value, _digits, out var written);

        return _digits.AsMemory(0, written);
    }

    // Everything the finished stream left behind, so the next one starts clean.
    private void Reset()
    {
        _header.Reset();

        _headerEntries.Clear();
        _queryEntries.Clear();
        _body.Reset();
        _response.Reset();
        _client.Reset();
        _properties.Clear();

        ResponseHeaders.Clear();

        _server = null;
        _endPoint = null;
        _hasBody = false;
        _bodyWrapper = null;
        _bodyFetched = false;
    }

    public IServer Server => _server ?? throw new InvalidOperationException("The request has not been applied to a stream.");

    public IEndPoint EndPoint => _endPoint ?? throw new InvalidOperationException("The request has not been applied to a stream.");

    public IClientConnection Client => _client;

    public IPropertyBag Properties => _properties;

    public IRequestHeader Header => _header;

    // The body, once - a stream cannot be read twice.
    public IRequestBody? GetBody(HeaderAccess headerAccess = HeaderAccess.Retain)
    {
        if (_bodyFetched)
        {
            throw new InvalidOperationException("Request body can only be fetched once.");
        }

        _bodyFetched = true;

        if (!_hasBody)
        {
            return null;
        }

        return _bodyWrapper is not null ? _bodyWrapper(_body) : _body;
    }

    // Lets a handler decorate the body before anyone reads it.
    public void WrapBody(Func<IRequestBody, IRequestBody> wrapper) => _bodyWrapper = wrapper;

    // A response builder for this request, starting at 200.
    public IResponseBuilder Respond() => _response.Status(ResponseStatus.Ok);

    // There is no upgrade here: both protocols carry their own stream multiplexing.
    public PipeReader Upgrade()
        => throw new NotSupportedException("Connection upgrades are not available over HTTP/2 or HTTP/3.");

    // Nothing of its own to release: the driver returns it to the pool once the stream is done.
    public ValueTask DisposeAsync() => new();

    // Splits the query out of :path, which is where both protocols carry it.
    private static void ParseQuery(ReadOnlyMemory<byte> path, List<(ReadOnlyMemory<byte>, ReadOnlyMemory<byte>)> into)
    {
        var mark = path.Span.IndexOf((byte)'?');

        if (mark < 0)
        {
            return;
        }

        var query = path[(mark + 1)..];

        while (!query.IsEmpty)
        {
            var end = query.Span.IndexOf((byte)'&');

            var pair = end < 0 ? query : query[..end];

            query = end < 0 ? default : query[(end + 1)..];

            if (pair.IsEmpty)
            {
                continue;
            }

            var equals = pair.Span.IndexOf((byte)'=');

            into.Add(equals < 0
                ? (pair, ReadOnlyMemory<byte>.Empty)
                : (pair[..equals], pair[(equals + 1)..]));
        }
    }

    // Whether the request declares a body, which over these protocols means a Content-Length header.
    private static bool HasBody(List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> headers)
    {
        foreach (var (name, _) in headers)
        {
            if (Ascii.EqualsIgnoreCase(name.Span, ContentLength.Span))
            {
                return true;
            }
        }

        return false;
    }
}
