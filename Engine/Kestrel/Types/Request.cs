using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Engine.Shared.Types;
using Microsoft.AspNetCore.Http;
using Local = GenHTTP.Engine.Kestrel.Hosting;

using HttpProtocol = GenHTTP.Api.Protocol.HttpProtocol;

namespace GenHTTP.Engine.Kestrel.Types;

public sealed class Request : IRequest
{
    private RequestProperties? _Properties;

    private Query? _Query;

    private Cookies? _Cookies;

    private Forwardings? _Forwardings;

    private Headers? _Headers;

    #region Get-/Setters

    public IRequestProperties Properties
    {
        get { return _Properties ??= new RequestProperties(); }
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
        get { return _Query ??= new Query(Context); }
    }

    public ICookieCollection Cookies
    {
        get { return _Cookies ??= new Cookies(Context); }
    }

    public IForwardingCollection Forwardings
    {
        get { return _Forwardings ??= new Forwardings(Context); }
    }

    public IHeaderCollection Headers
    {
        get { return _Headers ??= new Headers(Context); }
    }

    public Stream? Content => Context.BodyReader.AsStream(true);

    public FlexibleContentType? ContentType => (Context.ContentType != null) ? new(Context.ContentType) : null;

    private HttpRequest Context { get; }

    #endregion

    #region Initialization

    public Request(IServer server, HttpContext context)
    {
        Server = server;
        Context = context.Request;

        // todo
        ProtocolType = Context.Protocol == "HTTP/1.1" ? HttpProtocol.Http11 : HttpProtocol.Http10;
        Method = FlexibleRequestMethod.Get(Context.Method);
        Target = new RoutingTarget(WebPath.FromString(Context.Path));

        LocalClient = Client = new Local.ClientConnection(context.Connection, context.Request);

        // todo
        EndPoint = Server.EndPoints.First(e => e.Port == context.Connection.LocalPort);
    }

    #endregion

    #region Functionality

    public IResponseBuilder Respond() => new ResponseBuilder().Status(ResponseStatus.Ok);

    public UpgradeInfo Upgrade() => throw new NotImplementedException();

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
