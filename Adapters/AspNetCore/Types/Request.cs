using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Engine.Shared.Types;

using Microsoft.AspNetCore.Http;

using HttpProtocol = GenHTTP.Api.Protocol.HttpProtocol;

namespace GenHTTP.Adapters.AspNetCore.Types;

public sealed class Request : IRequest
{
    private readonly ResponseBuilder _responseBuilder;

    private bool _freshResponse = true;

    private IServer? _server;
    private IEndPoint? _endPoint;

    private IClientConnection? _clientConnection;
    private IClientConnection? _localClient;

    private FlexibleRequestMethod? _method;
    private RoutingTarget? _target;

    private readonly RequestProperties _properties = new();

    private readonly Cookies _cookies = new();

    private readonly ForwardingCollection _forwardings = new();

    private readonly Headers _headers = new();

    private readonly Query _query = new();

    #region Get-/Setters

    public IRequestProperties Properties => _properties;

    public IServer Server => _server ?? throw new InvalidOperationException("Request is not initialized yet");

    public IEndPoint EndPoint => _endPoint ?? throw new InvalidOperationException("Request is not initialized yet");

    public IClientConnection Client => _clientConnection ?? throw new InvalidOperationException("Request is not initialized yet");

    public IClientConnection LocalClient => _localClient ?? throw new InvalidOperationException("Request is not initialized yet");

    public HttpProtocol ProtocolType { get; private set; }

    public FlexibleRequestMethod Method => _method ?? throw new InvalidOperationException("Request is not initialized yet");

    public RoutingTarget Target => _target ?? throw new InvalidOperationException("Request is not initialized yet");

    public string? UserAgent => this["User-Agent"];

    public string? Referer => this["Referer"];

    public string? Host => this["Host"];

    public string? this[string additionalHeader] => Headers.GetValueOrDefault(additionalHeader);

    public IRequestQuery Query => _query;

    public ICookieCollection Cookies => _cookies;

    public IForwardingCollection Forwardings => _forwardings;

    public IHeaderCollection Headers => _headers;

    public Stream Content => Context?.BodyReader.AsStream(true) ?? throw new InvalidOperationException("Request is not initialized yet");

    public FlexibleContentType? ContentType => (Context?.ContentType != null) ? new(Context.ContentType) : null;

    private HttpRequest? Context { get; set; }

    #endregion

    #region Initialization

    public Request(ResponseBuilder responseBuilder)
    {
        _responseBuilder = responseBuilder;
    }

    #endregion

    #region Functionality

    public IResponseBuilder Respond()
    {
        if (!_freshResponse)
        {
            _responseBuilder.Reset();
        }
        else
        {
            _freshResponse = false;
        }

        return _responseBuilder;
    }

    public UpgradeInfo Upgrade() => throw new NotSupportedException("Web sockets are not supported by the Kestrel server implementation");

    internal void Configure(IServer server, HttpContext context)
    {
        var request = context.Request;

        _server = server;
        Context = request;

        ProtocolType = Context.Protocol switch
        {
            "HTTP/1.0" => HttpProtocol.Http10,
            "HTTP/1.1" => HttpProtocol.Http11,
            "HTTP/2" => HttpProtocol.Http2,
            "HTTP/3" => HttpProtocol.Http3,
            _ => HttpProtocol.Http11
        };

        _method = FlexibleRequestMethod.Get(Context.Method);
        _target = new RoutingTarget(WebPath.FromString(Context.Path));

        _query.SetRequest(request);
        _headers.SetRequest(request);
        _cookies.SetRequest(request);

        if (context.Request.Headers.TryGetValue("forwarded", out var forwardings))
        {
            foreach (var entry in forwardings)
            {
                if (entry != null) _forwardings.Add(entry);
            }
        }
        else
        {
            _forwardings.TryAddLegacy(Headers);
        }

        _localClient = new ClientConnection(context.Connection, request);

        _clientConnection = _forwardings.DetermineClient(context.Connection.ClientCertificate) ?? LocalClient;

        _endPoint = Server.EndPoints.First(e => e.Port == context.Connection.LocalPort);
    }

    internal void Reset()
    {
        _properties.Clear();
        _forwardings.Clear();
        _cookies.Clear();

        _query.SetRequest(null);
        _headers.SetRequest(null);

        _localClient = null;
        _clientConnection = null;
        _target = null;
        _method = null;
    }

    #endregion

    #region Lifecycle

    public void Dispose()
    {
        Reset();
    }

    #endregion

}
