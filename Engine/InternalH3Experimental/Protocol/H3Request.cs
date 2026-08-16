using System.IO.Pipelines;
using System.Net;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Shared.Types;

using Glyph3;

namespace GenHTTP.Engine.InternalH3Experimental.Protocol;

/// <summary>
/// An <see cref="IRequest"/> over a Glyph3 request.
/// </summary>
/// <remarks>
/// Not the shared <see cref="Request"/>, whose Source is a Glyph11 BinaryRequest and so assumes an
/// HTTP/1.1 parse. Nothing here is pooled: HTTP/3 multiplexes, so several of these are live on one
/// connection at once and a per-connection pool would need locking to be safe.
/// </remarks>
internal sealed class H3Request : IRequest
{
    private readonly H3RequestBody? _body;

    private readonly ClientConnection _client = new();

    private readonly PropertyBag _properties = new();

    private readonly ResponseBuilder _response = new();

    private Func<IRequestBody, IRequestBody>? _bodyWrapper;

    private IRequestBody? _wrappedBody;

    private bool _bodyFetched;

    internal H3Request(IServer server, IEndPoint endPoint, Http3Request source, ReadOnlyMemory<byte> body, IPAddress? remoteAddress)
    {
        Server = server;
        EndPoint = endPoint;

        Header = new H3RequestHeader(source, ParseQuery(source.Path));

        _body = body.IsEmpty ? null : new H3RequestBody(body);

        _client.Apply(remoteAddress, ClientProtocol.Https, null);
    }

    public IServer Server { get; }

    public IEndPoint EndPoint { get; }

    public IClientConnection Client => _client;

    public IPropertyBag Properties => _properties;

    public IRequestHeader Header { get; }

    public IRequestBody? GetBody(HeaderAccess headerAccess = HeaderAccess.Retain)
    {
        if (_bodyFetched)
        {
            throw new InvalidOperationException("Request body can only be fetched once.");
        }

        _bodyFetched = true;

        if (_body is null)
        {
            return null;
        }

        return _wrappedBody = _bodyWrapper is not null ? _bodyWrapper(_body) : _body;
    }

    public void WrapBody(Func<IRequestBody, IRequestBody> wrapper) => _bodyWrapper = wrapper;

    public IResponseBuilder Respond() => _response.Status(ResponseStatus.Ok);

    /// <summary>
    /// Not supported. Upgrading a request to a raw byte stream is an HTTP/1.1 mechanism, and QUIC
    /// streams are reached through the transport rather than through the request.
    /// </summary>
    public PipeReader Upgrade()
        => throw new NotSupportedException("Connection upgrades are not available over HTTP/3.");

    public ValueTask DisposeAsync() => new();

    // The query string, which HTTP/3 carries inside :path exactly as HTTP/1.1 carries it inside the
    // request target.
    private static List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> ParseQuery(ReadOnlyMemory<byte> path)
    {
        var parameters = new List<(ReadOnlyMemory<byte>, ReadOnlyMemory<byte>)>();

        int mark = path.Span.IndexOf((byte)'?');
        if (mark < 0)
        {
            return parameters;
        }

        ReadOnlyMemory<byte> query = path[(mark + 1)..];

        while (!query.IsEmpty)
        {
            int end = query.Span.IndexOf((byte)'&');
            ReadOnlyMemory<byte> pair = end < 0 ? query : query[..end];
            query = end < 0 ? default : query[(end + 1)..];

            if (pair.IsEmpty)
            {
                continue;
            }

            int equals = pair.Span.IndexOf((byte)'=');

            parameters.Add(equals < 0
                ? (pair, ReadOnlyMemory<byte>.Empty)
                : (pair[..equals], pair[(equals + 1)..]));
        }

        return parameters;
    }
}
