using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Engine.Internal.Utilities;
using GenHTTP.Engine.Shared.Types;

using CookieCollection = GenHTTP.Engine.Shared.Types.CookieCollection;

namespace GenHTTP.Engine.Internal.Protocol;

/// <summary>
/// Provides methods to access a recieved http request.
/// </summary>
internal sealed class Request : IRequest
{
    private readonly ResponseBuilder _responseBuilder;

    private bool _freshResponse = true;

    private IServer? _server;
    private IEndPoint? _endPoint;

    private IClientConnection? _clientConnection;
    private IClientConnection? _localClient;

    private Socket? _socket;
    private Stream? _stream;

    private FlexibleRequestMethod? _method;
    private RoutingTarget? _target;

    private readonly RequestHeaderCollection _headers = new();

    private readonly CookieCollection _cookies = new();

    private readonly ForwardingCollection _forwardings = new();

    private readonly RequestProperties _properties = new();

    private readonly RequestQuery _query = new();

    private Stream? _content;
    private FlexibleContentType? _contentType;

    #region Initialization

    internal Request(ResponseBuilder responseBuilder)
    {
        _responseBuilder = responseBuilder;
    }

    #endregion

    #region Get-/Setters

    public IServer Server => _server ?? throw new InvalidOperationException("Request is not initialized yet");

    public IEndPoint EndPoint => _endPoint ?? throw new InvalidOperationException("Request is not initialized yet");

    public IClientConnection Client => _clientConnection ?? throw new InvalidOperationException("Request is not initialized yet");

    public IClientConnection LocalClient => _localClient ?? throw new InvalidOperationException("Request is not initialized yet");

    public HttpProtocol ProtocolType { get; internal set; }

    public FlexibleRequestMethod Method => _method ?? throw new InvalidOperationException("Request is not initialized yet");

    public RoutingTarget Target => _target ?? throw new InvalidOperationException("Request is not initialized yet");

    public IHeaderCollection Headers => _headers;

    public Stream? Content => _content;

    public FlexibleContentType? ContentType
    {
        get
        {
            if (_contentType is not null)
            {
                return _contentType;
            }

            var type = this["Content-Type"];

            if (type is not null)
            {
                return _contentType = new FlexibleContentType(type);
            }

            return null;
        }
    }

    public string? Host => Client.Host;

    public string? Referer => this["Referer"];

    public string? UserAgent => this["User-Agent"];

    public string? this[string additionalHeader] => Headers.GetValueOrDefault(additionalHeader);

    public ICookieCollection Cookies => _cookies;

    public IForwardingCollection Forwardings => _forwardings;

    public IRequestQuery Query => _query;

    public IRequestProperties Properties => _properties;

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

    public UpgradeInfo Upgrade()
    {
        if (_socket == null || _stream == null)
        {
            throw new InvalidOperationException("Request is not initialized yet");
        }

        return new(_socket, _stream, new Response { Upgraded = true });
    }

    #endregion

    #region Parsing

    internal void SetConnection(IServer server, Socket connection, PoolBufferedStream stream, IEndPoint endPoint, IPAddress? address, X509Certificate? clientCertificate)
    {
        _server = server;
        _socket = connection;
        _stream = stream;
        _endPoint = endPoint;

        var protocol = _endPoint.Secure ? ClientProtocol.Https : ClientProtocol.Http;

        if (!Headers.TryGetValue("Host", out var host))
        {
            throw new ProtocolException("Mandatory 'Host' header is missing from the request");
        }

        if (_forwardings.Count == 0)
        {
            _forwardings.TryAddLegacy(Headers);
        }

        _localClient = new ClientConnection(address, protocol, host, clientCertificate);

        _clientConnection = _forwardings.DetermineClient(clientCertificate) ?? _localClient;
    }

    internal void SetHeader(string key, string value)
    {
        if (string.Equals(key, "cookie", StringComparison.OrdinalIgnoreCase))
        {
            _cookies.Add(value);
        }
        else if (string.Equals(key, "forwarded", StringComparison.OrdinalIgnoreCase))
        {
            _forwardings.Add(value);
        }
        else
        {
            _headers[key] = value;
        }
    }

    internal void SetContent(Stream content)
    {
        _content = content;
    }

    internal void SetProtocol(HttpProtocol protocol)
    {
        ProtocolType = protocol;
    }

    internal void SetPath(WebPath path)
    {
        _target = new(path);
    }

    internal void SetMethod(FlexibleRequestMethod method)
    {
        _method = method;
    }

    internal void SetQuery(string key, string value)
    {
        _query[key] = value;
    }

    internal void Reset()
    {
        _headers.Clear();
        _cookies.Clear();
        _forwardings.Clear();
        _properties.Clear();
        _query.Clear();

        Content?.Dispose();

        _content = null;
        _contentType = null;

        _freshResponse = true;
    }

    #endregion

    #region IDisposable Support

    private bool _disposed;

    public void Dispose()
    {
        if (!_disposed)
        {
            Content?.Dispose();

            _disposed = true;
        }
    }

    #endregion

}
