using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.ReverseProxy.Provider;

public sealed class ReverseProxyBuilder : IHandlerBuilder<ReverseProxyBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private TimeSpan _connectTimeout = TimeSpan.FromSeconds(10);
    private TimeSpan _readTimeout = TimeSpan.FromSeconds(60);

    private string? _upstream;

    private Action<SocketsHttpHandler>? _handlerAdjustments;
    private Action<HttpClient>? _clientAdjustments;

    #region Functionality

    /// <summary>
    /// Sets the server to pass the incoming requests to.
    /// </summary>
    /// <param name="upstream">The URL of the server to pass requests to</param>
    public ReverseProxyBuilder Upstream(string upstream)
    {
        _upstream = upstream;

        if (_upstream.EndsWith('/'))
        {
            _upstream = _upstream[..^1];
        }

        return this;
    }

    /// <summary>
    /// The time allowed to try connecting to the upstream server (defaults to 10 seconds).
    /// </summary>
    /// <param name="connectTimeout">The connection timeout to be set</param>
    public ReverseProxyBuilder ConnectTimeout(TimeSpan connectTimeout)
    {
        _connectTimeout = connectTimeout;
        return this;
    }

    /// <summary>
    /// The time allowed for the upstream server to generate a response (defaults to 60 seconds).
    /// </summary>
    /// <param name="readTimeout">The read timeout to be set</param>
    public ReverseProxyBuilder ReadTimeout(TimeSpan readTimeout)
    {
        _readTimeout = readTimeout;
        return this;
    }

    /// <summary>
    /// A callback to be invoked to configure the socket HTTP handler used by the handler.
    /// </summary>
    /// <param name="adjustment">The callback to be invoked</param>
    public ReverseProxyBuilder AdjustHandler(Action<SocketsHttpHandler> adjustment)
    {
        _handlerAdjustments = adjustment;
        return this;
    }

    /// <summary>
    /// A callback to be invoked to configure the HTTP client used by the handler.
    /// </summary>
    /// <param name="adjustment">The callback to be invoked</param>
    public ReverseProxyBuilder AdjustClient(Action<HttpClient> adjustment)
    {
        _clientAdjustments = adjustment;
        return this;
    }

    public ReverseProxyBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        if (_upstream is null)
        {
            throw new BuilderMissingPropertyException("Upstream");
        }

        var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            ConnectTimeout = _connectTimeout
        };

        _handlerAdjustments?.Invoke(handler);

        var client = new HttpClient(handler)
        {
            Timeout = _readTimeout
        };

        _clientAdjustments?.Invoke(client);

        return Concerns.Chain(_concerns, new ReverseProxyProvider(_upstream, client));
    }

    #endregion

}
