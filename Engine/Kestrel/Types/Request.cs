using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Engine.Shared.Types;

using HttpProtocol = GenHTTP.Api.Protocol.HttpProtocol;

namespace GenHTTP.Engine.Kestrel.Types;

public sealed class Request : IRequest
{

    #region Get-/Setters

    public IRequestProperties Properties { get; }

    public IServer Server { get; }

    public IEndPoint EndPoint { get; }

    public IClientConnection Client { get; }

    public IClientConnection LocalClient { get; }

    public HttpProtocol ProtocolType { get; }

    public FlexibleRequestMethod Method { get; }

    public RoutingTarget Target { get; }

    public string? UserAgent { get; }

    public string? Referer { get; }

    public string? Host { get; }

    public string? this[string additionalHeader] => throw new NotImplementedException();

    public IRequestQuery Query { get; }

    public ICookieCollection Cookies { get; }

    public IForwardingCollection Forwardings { get; }

    public IHeaderCollection Headers { get; }

    public Stream? Content { get; }

    public FlexibleContentType? ContentType { get; }

    #endregion

    #region Initialization

    public Request(HttpContext context)
    {

    }

    #endregion

    #region Functionality

    public IResponseBuilder Respond() => new ResponseBuilder().Status(ResponseStatus.Ok);

    public UpgradeInfo Upgrade() => throw new NotImplementedException();

    #endregion

    #region Lifecycle

    public void Dispose() { }

    #endregion

}
