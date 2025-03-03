﻿using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Engine.Internal.Protocol;

internal sealed class RequestBuilder : IBuilder<IRequest>
{
    private Socket? _Socket;
    private Stream? _Stream;

    private IPAddress? _Address;
    private X509Certificate? _ClientCertificate;

    private Stream? _Content;

    private Shared.Types.CookieCollection? _Cookies;
    private IEndPoint? _EndPoint;

    private ForwardingCollection? _Forwardings;
    private HttpProtocol? _Protocol;

    private RequestQuery? _Query;

    private FlexibleRequestMethod? _RequestMethod;
    private IServer? _Server;

    private RoutingTarget? _Target;

    #region Initialization

    internal RequestBuilder()
    {
        Headers = new RequestHeaderCollection();
    }

    #endregion

    #region Get-/Setters

    private Shared.Types.CookieCollection Cookies
    {
        get { return _Cookies ??= new Shared.Types.CookieCollection(); }
    }

    private ForwardingCollection Forwardings
    {
        get { return _Forwardings ??= new ForwardingCollection(); }
    }

    internal RequestHeaderCollection Headers { get; }

    #endregion

    #region Functionality

    public RequestBuilder Connection(IServer server, Socket socket, Stream stream, IEndPoint endPoint, IPAddress? address, X509Certificate? clientCertificate)
    {
        _Socket = socket;
        _Stream = stream;

        _Server = server;
        _Address = address;
        _EndPoint = endPoint;
        _ClientCertificate = clientCertificate;

        return this;
    }

    public RequestBuilder Protocol(HttpProtocol version)
    {
        _Protocol = version;
        return this;
    }

    public RequestBuilder Type(FlexibleRequestMethod type)
    {
        _RequestMethod = type;
        return this;
    }

    public RequestBuilder Path(WebPath path)
    {
        _Target = new RoutingTarget(path);
        return this;
    }

    public RequestBuilder Query(RequestQuery query)
    {
        _Query = query;
        return this;
    }

    public RequestBuilder Header(string key, string value)
    {
        if (string.Equals(key, "cookie", StringComparison.OrdinalIgnoreCase))
        {
            Cookies.Add(value);
        }
        else if (string.Equals(key, "forwarded", StringComparison.OrdinalIgnoreCase))
        {
            Forwardings.Add(value);
        }
        else
        {
            Headers[key] = value;
        }

        return this;
    }

    public RequestBuilder Content(Stream content)
    {
        _Content = content;
        return this;
    }

    public IRequest Build()
    {
        try
        {
            if (_Server is null)
            {
                throw new BuilderMissingPropertyException("Server");
            }

            if (_Socket == null)
            {
                throw new BuilderMissingPropertyException("Socket");
            }

            if (_Stream == null)
            {
                throw new BuilderMissingPropertyException("Stream");
            }

            if (_EndPoint is null)
            {
                throw new BuilderMissingPropertyException("EndPoint");
            }

            if (_Address is null)
            {
                throw new BuilderMissingPropertyException("Address");
            }

            if (_Protocol is null)
            {
                throw new BuilderMissingPropertyException("Protocol");
            }

            if (_RequestMethod is null)
            {
                throw new BuilderMissingPropertyException("Type");
            }

            if (_Target is null)
            {
                throw new BuilderMissingPropertyException("Target");
            }

            var protocol = _EndPoint.Secure ? ClientProtocol.Https : ClientProtocol.Http;

            if (!Headers.TryGetValue("Host", out var host))
            {
                throw new ProtocolException("Mandatory 'Host' header is missing from the request");
            }

            if (_Forwardings is null)
            {
                Forwardings.TryAddLegacy(Headers);
            }

            var localClient = new ClientConnection(_Address, protocol, host, _ClientCertificate);

            var client = Forwardings.DetermineClient(_ClientCertificate) ?? localClient;

            return new Request(_Server, _Socket, _Stream, _EndPoint, client, localClient, (HttpProtocol)_Protocol, _RequestMethod,
                               _Target, Headers, _Cookies, _Forwardings, _Query, _Content);
        }
        catch (Exception)
        {
            Headers.Dispose();

            _Query?.Dispose();
            _Cookies?.Dispose();

            throw;
        }
    }

    #endregion

}
