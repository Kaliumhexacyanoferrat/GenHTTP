using System.IO.Pipelines;
using System.Net;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Engine.Ioxide.Protocol.Multiplexed;

internal sealed class MultiplexedRequest : IRequest
{
    private readonly MultiplexedRequestBody? _body;

    private readonly ClientConnection _client = new();

    private readonly PropertyBag _properties = new();

    private readonly ResponseBuilder _response = new();

    private Func<IRequestBody, IRequestBody>? _bodyWrapper;

    private bool _bodyFetched;

    internal MultiplexedRequest(IServer server, IEndPoint endPoint, ReadOnlyMemory<byte> method, ReadOnlyMemory<byte> path,
        ReadOnlyMemory<byte> authority, List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> headers,
        Func<ValueTask<ReadOnlyMemory<byte>>>? read, IPAddress? remoteAddress, HttpProtocol protocol, bool secure)
    {
        Server = server;
        EndPoint = endPoint;

        Header = new MultiplexedRequestHeader(method, path, authority, headers, ParseQuery(path), protocol);

        _body = read is null ? null : new MultiplexedRequestBody(read);

        _client.Apply(remoteAddress, secure ? ClientProtocol.Https : ClientProtocol.Http, null);
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

        return _bodyWrapper is not null ? _bodyWrapper(_body) : _body;
    }

    public void WrapBody(Func<IRequestBody, IRequestBody> wrapper) => _bodyWrapper = wrapper;

    public IResponseBuilder Respond() => _response.Status(ResponseStatus.Ok);

    public PipeReader Upgrade()
        => throw new NotSupportedException("Connection upgrades are not available over HTTP/2 or HTTP/3.");

    public ValueTask DisposeAsync() => new();

    private static List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> ParseQuery(ReadOnlyMemory<byte> path)
    {
        var parameters = new List<(ReadOnlyMemory<byte>, ReadOnlyMemory<byte>)>();

        var mark = path.Span.IndexOf((byte)'?');

        if (mark < 0)
        {
            return parameters;
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

            parameters.Add(equals < 0
                ? (pair, ReadOnlyMemory<byte>.Empty)
                : (pair[..equals], pair[(equals + 1)..]));
        }

        return parameters;
    }
}
