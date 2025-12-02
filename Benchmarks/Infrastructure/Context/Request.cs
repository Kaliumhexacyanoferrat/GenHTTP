using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Benchmarks.Infrastructure.Context;

public class Request : IRequest
{
    private readonly ResponseBuilder _responseBuilder;

    private bool _freshResponse = true;

    #region Get-/Setters

    public IRequestProperties Properties { get; set; }

    public IServer Server { get; set; }

    public IEndPoint EndPoint { get; set; }

    public IClientConnection Client { get; set; }

    public IClientConnection LocalClient { get; set; }

    public HttpProtocol ProtocolType { get; set; }

    public FlexibleRequestMethod Method { get; set; }

    public RoutingTarget Target { get; set; }

    public string? UserAgent => this["UserAgent"];

    public string? Referer => this["Referer"];

    public string? Host => this["Host"];

    public string? this[string additionalHeader] => Headers[additionalHeader];

    public IRequestQuery Query { get; set; }

    public ICookieCollection Cookies { get; set; }

    public IForwardingCollection Forwardings { get; set; }

    public IHeaderCollection Headers { get; set; }

    public Stream? Content { get; set; }

    public FlexibleContentType? ContentType { get; set; }

    #endregion

    #region Initialization

    public Request(ResponseBuilder responseBuilder)
    {
        _responseBuilder = responseBuilder;

        Server = new BenchmarkServer();
        EndPoint = Server.EndPoints[0];

        Method = new FlexibleRequestMethod(RequestMethod.Get);
        Target = new RoutingTarget(WebPath.FromString("/"));

        Properties = new Properties();
        Query = new Query();
        Headers = new Headers();
        Forwardings = new Forwardings();
        Cookies =  new Cookies();

        LocalClient = Client = new ClientConnection();
    }

    #endregion

    #region Functionality

    public IResponseBuilder Respond()
    {
        if (_freshResponse)
        {
            _freshResponse = false;
        }
        else
        {
            _responseBuilder.Reset();
        }

        return _responseBuilder;
    }

    public UpgradeInfo Upgrade() => throw new NotSupportedException();

    public void Dispose()
    {
        // nop
    }

    #endregion

}
