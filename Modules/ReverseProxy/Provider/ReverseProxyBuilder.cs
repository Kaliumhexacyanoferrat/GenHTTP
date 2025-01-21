using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.ReverseProxy.Provider;

public sealed class ReverseProxyBuilder : IHandlerBuilder<ReverseProxyBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private TimeSpan _ConnectTimeout = TimeSpan.FromSeconds(10);
    private TimeSpan _ReadTimeout = TimeSpan.FromSeconds(60);

    private string? _Upstream;

    private Action<SocketsHttpHandler>? _HandlerAdjustments;
    private Action<HttpClient>? _ClientAdjustments;

    #region Functionality

    /// <summary>
    /// Sets the server to pass the incoming requests to.
    /// </summary>
    /// <param name="upstream">The URL of the server to pass requests to</param>
    public ReverseProxyBuilder Upstream(string upstream)
    {
        _Upstream = upstream;

        if (_Upstream.EndsWith('/'))
        {
            _Upstream = _Upstream[..^1];
        }

        return this;
    }

    /// <summary>
    /// The time allowed to try connecting to the upstream server (defaults to 10 seconds).
    /// </summary>
    /// <param name="connectTimeout">The connection timeout to be set</param>
    public ReverseProxyBuilder ConnectTimeout(TimeSpan connectTimeout)
    {
        _ConnectTimeout = connectTimeout;
        return this;
    }

    /// <summary>
    /// The time allowed for the upstream server to generate a response (defaults to 60 seconds).
    /// </summary>
    /// <param name="readTimeout">The read timeout to be set</param>
    public ReverseProxyBuilder ReadTimeout(TimeSpan readTimeout)
    {
        _ReadTimeout = readTimeout;
        return this;
    }

    /// <summary>
    /// A callback to be invoked to configure the socket HTTP handler used by the handler.
    /// </summary>
    /// <param name="adjustment">The callback to be invoked</param>
    public ReverseProxyBuilder AdjustHandler(Action<SocketsHttpHandler> adjustment)
    {
        _HandlerAdjustments = adjustment;
        return this;
    }

    /// <summary>
    /// A callback to be invoked to configure the HTTP client used by the handler.
    /// </summary>
    /// <param name="adjustment">The callback to be invoked</param>
    public ReverseProxyBuilder AdjustClient(Action<HttpClient> adjustment)
    {
        _ClientAdjustments = adjustment;
        return this;
    }

    public ReverseProxyBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        if (_Upstream is null)
        {
            throw new BuilderMissingPropertyException("Upstream");
        }

        var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            ConnectTimeout = _ConnectTimeout
        };

        _HandlerAdjustments?.Invoke(handler);

        var client = new HttpClient(handler)
        {
            Timeout = _ReadTimeout
        };

        _ClientAdjustments?.Invoke(client);

        return Concerns.Chain(_Concerns, new ReverseProxyProvider(_Upstream, client));
    }

    #endregion

}
