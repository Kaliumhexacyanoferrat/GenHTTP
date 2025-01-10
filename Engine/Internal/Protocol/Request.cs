using System.Net.Sockets;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Engine.Internal.Protocol;

/// <summary>
/// Provides methods to access a recieved http request.
/// </summary>
internal sealed class Request : IRequest
{
    private readonly Socket _Socket;
    private readonly Stream _Stream;

    private FlexibleContentType? _ContentType;
    private ICookieCollection? _Cookies;

    private IForwardingCollection? _Forwardings;

    private RequestProperties? _Properties;

    private IRequestQuery? _Query;

    #region Initialization

    internal Request(IServer server, Socket socket, Stream stream, IEndPoint endPoint, IClientConnection client, IClientConnection localClient, HttpProtocol protocol, FlexibleRequestMethod method,
        RoutingTarget target, IHeaderCollection headers, ICookieCollection? cookies, IForwardingCollection? forwardings,
        IRequestQuery? query, Stream? content)
    {
        _Socket = socket;
        _Stream = stream;

        Client = client;
        LocalClient = localClient;

        Server = server;
        EndPoint = endPoint;

        ProtocolType = protocol;
        Method = method;
        Target = target;

        _Cookies = cookies;
        _Forwardings = forwardings;
        _Query = query;

        Headers = headers;

        Content = content;
    }

    #endregion

    #region Functionality

    public IResponseBuilder Respond() => new ResponseBuilder().Status(ResponseStatus.Ok);

    public UpgradeInfo Upgrade() => new(_Socket, _Stream, new Response { Upgraded = true });

    #endregion

    #region Get-/Setters

    public IServer Server { get; }

    public IEndPoint EndPoint { get; }

    public IClientConnection Client { get; }

    public IClientConnection LocalClient { get; }

    public HttpProtocol ProtocolType { get; }

    public FlexibleRequestMethod Method { get; }

    public RoutingTarget Target { get; }

    public IHeaderCollection Headers { get; }

    public Stream? Content { get; }

    public FlexibleContentType? ContentType
    {
        get
        {
            if (_ContentType is not null)
            {
                return _ContentType;
            }

            var type = this["Content-Type"];

            if (type is not null)
            {
                return _ContentType = new FlexibleContentType(type);
            }

            return null;
        }
    }

    public string? Host => Client.Host;

    public string? Referer => this["Referer"];

    public string? UserAgent => this["User-Agent"];

    public string? this[string additionalHeader] => Headers.GetValueOrDefault(additionalHeader);

    public ICookieCollection Cookies
    {
        get { return _Cookies ??= new CookieCollection(); }
    }

    public IForwardingCollection Forwardings
    {
        get { return _Forwardings ??= new ForwardingCollection(); }
    }

    public IRequestQuery Query
    {
        get { return _Query ??= new RequestQuery(); }
    }

    public IRequestProperties Properties
    {
        get { return _Properties ??= new RequestProperties(); }
    }

    #endregion

    #region IDisposable Support

    private bool _Disposed;

    public void Dispose()
    {
        if (!_Disposed)
        {
            Headers.Dispose();

            _Query?.Dispose();

            _Cookies?.Dispose();

            _Properties?.Dispose();

            Content?.Dispose();

            _Disposed = true;
        }
    }

    #endregion

}
