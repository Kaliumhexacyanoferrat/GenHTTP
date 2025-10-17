using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Engine.Shared.Types;
using HttpProtocol = GenHTTP.Api.Protocol.HttpProtocol;

namespace GenHTTP.Adapters.WiredIO.Types;

public sealed class Request : IRequest
{
    private RequestProperties? _Properties;

    private Query? _Query;

    private Cookies? _Cookies;

    private readonly ForwardingCollection _Forwardings = new();

    private Headers? _Headers;

    private readonly IEndPoint? _EndPoint;

    #region Get-/Setters

    public IRequestProperties Properties
    {
        get { return _Properties ??= new RequestProperties(); }
    }

    public IServer Server { get; }

    public IEndPoint EndPoint => _EndPoint ?? throw new InvalidOperationException("EndPoint is not available as it is managed by ASP.NET Core");

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
        get { return _Query ??= new Query(Context); }
    }

    public ICookieCollection Cookies
    {
        get { return _Cookies ??= new Cookies(Context); }
    }

    public IForwardingCollection Forwardings => _Forwardings;

    public IHeaderCollection Headers
    {
        get { return _Headers ??= new Headers(Context); }
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
                if (entry != null) _Forwardings.Add(entry);
            }
        }
        else
        {
            _Forwardings.TryAddLegacy(Headers);
        }

        LocalClient = new ClientConnection(context.Connection, context.Request);

        Client = _Forwardings.DetermineClient(context.Connection.ClientCertificate) ?? LocalClient;

        _EndPoint = Server.EndPoints.FirstOrDefault(e => e.Port == context.Connection.LocalPort);
    }

    #endregion

    #region Functionality

    public IResponseBuilder Respond() => new ResponseBuilder().Status(ResponseStatus.Ok);

    public UpgradeInfo Upgrade() => throw new NotSupportedException("Web sockets are not supported by the Kestrel server implementation");

    #endregion

    #region Lifecycle

    private bool _Disposed;

    public void Dispose()
    {
        if (!_Disposed)
        {
            _Properties?.Dispose();

            _Disposed = true;
        }
    }

    #endregion

}
