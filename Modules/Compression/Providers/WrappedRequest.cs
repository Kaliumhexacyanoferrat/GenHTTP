using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Compression.Providers;

/// <summary>
/// Wraps an existing request to provide decompressed content stream.
/// </summary>
internal sealed class WrappedRequest : IRequest
{
    private readonly IRequest _inner;

    private readonly Stream _decompressedContent;

    #region Initialization

    public WrappedRequest(IRequest inner, Stream decompressedContent)
    {
        _inner = inner;
        _decompressedContent = decompressedContent;
    }

    #endregion

    #region Delegated Properties

    public IRequestProperties Properties => _inner.Properties;

    public IServer Server => _inner.Server;

    public IEndPoint EndPoint => _inner.EndPoint;

    public IClientConnection Client => _inner.Client;

    public IClientConnection LocalClient => _inner.LocalClient;

    public HttpProtocol ProtocolType => _inner.ProtocolType;

    public FlexibleRequestMethod Method => _inner.Method;

    public RoutingTarget Target => _inner.Target;

    public string? UserAgent => _inner.UserAgent;

    public string? Referer => _inner.Referer;

    public string? Host => _inner.Host;

    public string? this[string additionalHeader] => _inner[additionalHeader];

    public IRequestQuery Query => _inner.Query;

    public ICookieCollection Cookies => _inner.Cookies;

    public IForwardingCollection Forwardings => _inner.Forwardings;

    public IHeaderCollection Headers => _inner.Headers;

    public FlexibleContentType? ContentType => _inner.ContentType;

    #endregion

    #region Overridden Content

    public Stream? Content => _decompressedContent;

    #endregion

    #region Delegated Methods

    public IResponseBuilder Respond() => _inner.Respond();

    public UpgradeInfo Upgrade() => _inner.Upgrade();

    public void Dispose()
    {
        _decompressedContent.Dispose();
    }

    #endregion

}
