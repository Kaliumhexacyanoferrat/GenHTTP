using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Engine.Shared.Types;

using Microsoft.AspNetCore.Http;

using HttpProtocol = GenHTTP.Api.Protocol.HttpProtocol;

namespace GenHTTP.Adapters.AspNetCore.Types;

public sealed class Request : IRequest
{
    private RequestProperties? _properties;

    private Query? _query;

    private Cookies? _cookies;

    private readonly ForwardingCollection _forwardings = new();

    private Headers? _headers;

    #region Get-/Setters

    public IRequestProperties Properties
    {
        get { return _properties ??= new RequestProperties(); }
    }

    public IServer Server { get; }

    public IEndPoint EndPoint { get; }
    
    public IClientConnection Client { get; }

    public IClientConnection LocalClient { get; }

    public HttpProtocol ProtocolType { get; }

    public FlexibleRequestMethod Method { get; }

    public RoutingTarget Target { get; }

    public string? UserAgent => this["User-Agent"];

    public string? Referer => this["Referer"];

    public string? Host => this["Host"];

    public string? this[string additionalHeader] => Headers.GetValueOrDefault(additionalHeader);

    public IRequestQuery Query
    {
        get { return _query ??= new Query(Context); }
    }

    public ICookieCollection Cookies
    {
        get { return _cookies ??= new Cookies(Context); }
    }

    public IForwardingCollection Forwardings => _forwardings;

    public IHeaderCollection Headers
    {
        get { return _headers ??= new Headers(Context); }
    }

    public Stream Content => Context.BodyReader.AsStream(true);

    public FlexibleContentType? ContentType => (Context.ContentType != null) ? new(Context.ContentType) : null;

    private HttpRequest Context { get; }

    #endregion

    #region Initialization

    public Request(IServer server, HttpContext context)
    {
        Server = server;
        Context = context.Request;

        ProtocolType = Context.Protocol switch
        {
            "HTTP/1.0" => HttpProtocol.Http10,
            "HTTP/1.1" => HttpProtocol.Http11,
            "HTTP/2" => HttpProtocol.Http2,
            "HTTP/3" => HttpProtocol.Http3,
            _ => HttpProtocol.Http11
        };

        Method = FlexibleRequestMethod.Get(Context.Method);
        Target = new RoutingTarget(WebPath.FromString(Context.Path));

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

        LocalClient = new ClientConnection(context.Connection, context.Request);

        Client = _forwardings.DetermineClient(context.Connection.ClientCertificate) ?? LocalClient;

        EndPoint = Server.EndPoints.First(e => e.Port == context.Connection.LocalPort);
    }

    #endregion

    #region Functionality

    public IResponseBuilder Respond() => new ResponseBuilder().Status(ResponseStatus.Ok);

    public UpgradeInfo Upgrade() => throw new NotSupportedException("Web sockets are not supported by the Kestrel server implementation");

    #endregion

    #region Lifecycle

    private bool _disposed;

    public void Dispose()
    {
        if (!_disposed)
        {
            _properties?.Dispose();

            _disposed = true;
        }
    }

    #endregion

}
