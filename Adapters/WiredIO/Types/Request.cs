using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Engine.Shared.Types;

using Wired.IO.Http11Express.Request;

namespace GenHTTP.Adapters.WiredIO.Types;

public sealed class Request : IRequest
{
    private RequestProperties? _Properties;

    private Query? _Query;

    private Cookies? _Cookies;

    private readonly ForwardingCollection _Forwardings = new();

    private Headers? _Headers;

    #region Get-/Setters

    public IRequestProperties Properties
    {
        get { return _Properties ??= new RequestProperties(); }
    }

    public IServer Server { get; }

    public IEndPoint EndPoint => throw new InvalidOperationException("EndPoint is not available as it is managed by WiredIO");

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
        get { return _Query ??= new Query(InnerRequest); }
    }

    public ICookieCollection Cookies
    {
        get { return _Cookies ??= new Cookies(InnerRequest); }
    }

    public IForwardingCollection Forwardings => _Forwardings;

    public IHeaderCollection Headers
    {
        get { return _Headers ??= new Headers(InnerRequest); }
    }

    // todo: this is quite inefficient
    public Stream Content => (InnerRequest.Content != null) ? new MemoryStream(InnerRequest.Content) : Stream.Null;

    public FlexibleContentType? ContentType
    {
        get
        {
            if (InnerRequest.Headers.TryGetValue("Content-Type", out var contentType))
            {
                return FlexibleContentType.Parse(contentType);
            }

            return null;
        }
    }

    private IExpressRequest InnerRequest { get; }

    #endregion

    #region Initialization

    public Request(IServer server, IExpressRequest request)
    {
        Server = server;
        InnerRequest = request;

        // todo: no API provided by wired
        ProtocolType = HttpProtocol.Http11;

        Method = FlexibleRequestMethod.Get(request.HttpMethod);
        Target = new RoutingTarget(WebPath.FromString(request.Route));

        if (request.Headers.TryGetValue("forwarded", out var entry))
        {
            _Forwardings.Add(entry);
        }
        else
        {
            _Forwardings.TryAddLegacy(Headers);
        }

        LocalClient = new ClientConnection(request);

        // todo: potential client certificate is not exposed by wired
        Client = _Forwardings.DetermineClient(null) ?? LocalClient;
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
