using System.Diagnostics.CodeAnalysis;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Websockets.Handler;

public class ClonedRequest : IRequest
{
    
    #region Get-/Setters
    
    public IServer Server { get; }

    public IEndPoint EndPoint { get; }

    public IClientConnection Client { get; }

    public IClientConnection LocalClient { get; }

    public HttpProtocol ProtocolType { get; }

    public FlexibleRequestMethod Method { get; }

    public RoutingTarget Target { get; }

    public string? UserAgent => this["UserAgent"];

    public string? Referer => this["Referer"];

    public string? Host => this["Host"];

    public string? this[string additionalHeader] => throw new NotImplementedException();

    public IRequestQuery Query { get; }

    public ICookieCollection Cookies { get; }

    public IForwardingCollection Forwardings { get; }

    public IHeaderCollection Headers { get; }

    public Stream? Content => null;

    public FlexibleContentType? ContentType { get; }
    
    public IRequestProperties Properties { get; }
    
    #endregion
    
    #region Initialization

    public static ClonedRequest From(IRequest request)
    {
        var query = new RequestQuery(request.Query);
        var cookies = new CookieCollection(request.Cookies);
        var headers = new HeaderCollection(request.Headers);
        var forwardings = new ForwardingCollection(request.Forwardings);
        var properties = new RequestProperties();
        
        return new ClonedRequest(request.Server, request.EndPoint, request.Client, request.LocalClient,
            request.ProtocolType, request.Method, request.Target, query, cookies, forwardings, headers, 
            request.ContentType, properties);
    }

    private ClonedRequest(IServer server, IEndPoint endpoint, IClientConnection client, IClientConnection localClient,
        HttpProtocol protocol, FlexibleRequestMethod method, RoutingTarget target, IRequestQuery query,
        ICookieCollection cookies, IForwardingCollection forwardings, IHeaderCollection headers,
        FlexibleContentType? contentType, IRequestProperties properties)
    {
        Server = server;
        EndPoint = endpoint;
        Client = client;
        LocalClient = localClient;
        ProtocolType = protocol;
        Method = method;
        Target = target;
        Cookies = cookies;
        Forwardings = forwardings;
        Query = query;
        Properties = properties;
        Headers = headers;
        ContentType = contentType;
    }
    
    #endregion

    #region Functionality
    
    public IResponseBuilder Respond() => throw new NotSupportedException();

    public UpgradeInfo Upgrade() => throw new NotSupportedException();
    
    public void Dispose()
    {
        // nop
    }
    
    #endregion
    

}

internal class RequestQuery : Dictionary<string, string>, IRequestQuery
{

    internal RequestQuery(IRequestQuery query)
    {
        foreach (var pair in query)
        {
            Add(pair.Key, pair.Value);
        }
    }
    
    public void Dispose()
    {
        // nop
    }
    
}

internal class CookieCollection : Dictionary<string, Cookie>, ICookieCollection
{
    
    internal CookieCollection(ICookieCollection cookies)
    {
        foreach (var pair in cookies)
        {
            Add(pair.Key, pair.Value);
        }
    }
    
    public void Dispose()
    {
        // nop
    }
    
}


internal class HeaderCollection : Dictionary<string, string>, IHeaderCollection
{
    
    internal HeaderCollection(IHeaderCollection header)
    {
        foreach (var pair in header)
        {
            Add(pair.Key, pair.Value);
        }
    }
    
    public void Dispose()
    {
        // nop
    }
    
}

internal class ForwardingCollection : List<Forwarding>, IForwardingCollection
{
    
    internal ForwardingCollection(IForwardingCollection forwardings)
    {
        foreach (var forwarding in forwardings)
        {
            Add(forwarding);
        }
    }
    
    public void Dispose()
    {
        // nop
    }
    
}

internal class RequestProperties : IRequestProperties
{
    
    public object this[string key]
    {
        get => throw new KeyNotFoundException();
        set => throw new NotSupportedException();
    }

    public bool TryGet<T>(string key, [MaybeNullWhen(false)] out T entry)
    {
        entry = default;
        return false;
    }

    public void Clear(string key)
    {
        // nop
    }
    
    public void Dispose()
    {
        // nop
    }
    
}
