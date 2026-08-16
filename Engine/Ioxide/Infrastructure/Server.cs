using System.Diagnostics;
using System.IO.Pipelines;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

using GenHTTP.Engine.Ioxide.Protocol;
using GenHTTP.Engine.Ioxide.Protocol.Multiplexed;
using GenHTTP.Engine.Shared.Infrastructure;
using GenHTTP.Engine.Shared.Types;

using ioxide;
using ioxide.tls;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Ioxide.Infrastructure;

/// <summary>
/// Hosts an application on ioxide's io_uring reactors: one per core, each owning a ring and its
/// connections on its own thread. Protocols are per port; TLS termination and the QUIC listener
/// live in the other halves of this class.
/// </summary>
public sealed partial class Server : IServer
{
    private readonly ServerConfiguration _config;

    private readonly EndPoint _primary;

    private readonly Dictionary<ushort, EndPoint> _endPointByPort;

    private readonly Dictionary<ushort, SecurityConfiguration> _secure;

    private readonly ushort[] _tcpRequested;

    private readonly EndPoint? _quicRequested;

    private readonly Dictionary<ushort, IoxideProtocols> _protocols;

    private readonly Action<Reactor>? _onReactorStart;

    private readonly IoxideOptions _options;

    private readonly ILogger _logger;

    private Thread[]? _threads;

    private Reactor[]? _reactors;

    public string Version { get; } = typeof(Server).Assembly.GetName().Version?.ToString() ?? "0.1";

    public bool Running { get; private set; }

    public bool Development => _config.DevelopmentMode;

    public IPropertyBag Properties { get; } = new PropertyBag();

    public ILoggerFactory Logging => _config.Logging;

    public IEndPointCollection EndPoints { get; }

    public IHandler Handler { get; }

    internal Server(
        ServerConfiguration config, 
        IHandler handler,
        Action<Reactor>? onReactorStart = null,
        IoxideOptions? options = null)
    {
        _config = config;
        Handler = handler;
        _onReactorStart = onReactorStart;
        _options = options ?? IoxideOptions.Default;

        _logger = config.Logging.CreateLogger<Server>();

        var mapped = MapEndPoints(config);

        _primary = mapped[0];
        _endPointByPort = mapped.ToDictionary(e => e.Port);

        _protocols = mapped.ToDictionary(e => e.Port, e => ResolveProtocols(_options, config, e.Port));

        // Which endpoints want which listener, settled here so StartAsync only has to act on it.
        // Order matters: both read _protocols, and the TCP one reads _primary as well.
        _tcpRequested = ResolveTcpPorts();
        _quicRequested = ResolveQuicEndPoint(mapped);

        // Certificates are resolved per reactor in OnStart, not here - see ResolveTls.
        _secure = config.EndPoints
                        .Where(e => e.Security is not null)
                        .ToDictionary(e => e.Port, e => e.Security!);

        EndPoints = new EndPointCollection(mapped.Cast<IEndPoint>().ToList());
    }

    /// <summary>
    /// GenHTTP's endpoints as the engine's own, which is also where the one thing every endpoint
    /// must agree on is checked: ioxide binds the whole server with a single dual-stack mode.
    /// </summary>
    private static List<EndPoint> MapEndPoints(ServerConfiguration config)
    {
        var mapped = config.EndPoints
                           .Select(e => new EndPoint(e.Address, e.Port, e.DualStack, e.Security != null))
                           .ToList();

        if (mapped.Any(e => e.DualStack != mapped[0].DualStack))
        {
            throw new NotSupportedException("The ioxide engine binds all endpoints with one dual-stack mode.");
        }

        return mapped;
    }

    /// <summary>
    /// The engine-wide configuration, straight from the options. The listeners are added on top by
    /// <c>WithTcp</c> and <c>WithQuic</c>, which take their ports from the endpoint bindings.
    /// </summary>
    private ServerConfig BuildServerConfig() => new()
    {
        ReactorCount = _options.Reactor.ReactorCount,
        RingEntries = _options.Reactor.RingEntries,
        RecvBufferSize = _options.Reactor.RecvBufferSize,
        RecvSlots = _options.Reactor.RecvSlots,
        Incremental = _options.Reactor.Incremental,

        // Server-wide rather than per transport: it applies to the TCP listener and the UDP socket
        // alike, which is why the engine binds every endpoint with one mode.
        DualStack = _primary.DualStack,

        // No listeners yet - WithTcp and WithQuic add the ones the bindings ask for. Explicitly
        // null because ioxide's own default is a live listener on 8080, which an HTTP/3-only server
        // would otherwise inherit and bind for a protocol it does not serve.
        Tcp = null,
    };

    public async ValueTask StartAsync()
    {
        await PrepareHandlerAsync();

        Running = true;
        
        var serverConfig = BuildServerConfig();

        if (_tcpRequested.Length > 0)
        {
            serverConfig = WithTcp(serverConfig);
        }

        if (_quicRequested is { } quicEndPoint)
        {
            serverConfig = WithQuic(serverConfig, quicEndPoint);
        }

        _threads = new Thread[serverConfig.ReactorCount];
        _reactors = new Reactor[serverConfig.ReactorCount];

        // Reactors bind their listeners on their own threads, so StartAsync must not return before
        // they accept - a client connecting immediately (as the test host does) would otherwise
        // race the bind and get "connection refused".
        var listening = new CountdownEvent(serverConfig.ReactorCount);

        for (var i = 0; i < _threads.Length; i++)
        {
            var reactor = new Reactor(i, serverConfig)
            {
                OnStart = r =>
                {
                    IoxideReactor.Bind(r);

                    if (_secure.Count > 0)
                    {
                        var registry = new TlsRegistry();

                        foreach (var (port, options) in ResolveTls())
                        {
                            registry.Add(port, TlsService.Start(r, options, register: false));
                        }

                        r.AddService(registry);
                    }

                    _onReactorStart?.Invoke(r);
                    listening.Signal();
                },
                TcpHandle = (_, tcpConnection) => 
                    ConnectionDriver.HandleAsync(
                        this, 
                        _endPointByPort[tcpConnection.ListenerPort], 
                        tcpConnection, 
                        ProtocolsFor(tcpConnection.ListenerPort)),
                
                QuicHandle = _quicEngine is not null 
                    ? (_, quicConnection) => Http3Driver.RunAsync(this, _quicEndPoint!, quicConnection, _h3Options!) 
                    : null
            };

            _reactors[i] = reactor;

            _threads[i] = new Thread(reactor.Run)
            {
                Name = $"ioxide-genhttp-{i}",
                IsBackground = true,
            };

            _threads[i].Start();
        }

        // Off the caller. The timeout is a safety net for a reactor that fails to bind: log and
        // continue rather than hang the host forever.
        if (await Task.Run(() => listening.Wait(TimeSpan.FromSeconds(10))))
        {
            listening.Dispose();
        }
        else
        {
            _logger.LogWarning("Not all reactors reported listening within 10s; the server may not be fully accepting yet.");
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Listening on {Address}:{Port} ({Settings})", _primary.Address, _primary.Port, DescribeSettings());
        }
    }

    private async ValueTask PrepareHandlerAsync()
    {
        try
        {
            var start = Stopwatch.GetTimestamp();

            await Handler.PrepareAsync(this);

            var elapsed = Stopwatch.GetElapsedTime(start);

            _logger.LogInformation("Prepared handlers in {ElapsedMs:0.##} ms", elapsed.TotalMilliseconds);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Failed to prepare the handler chain");
        }
    }
    
    /// <summary>
    /// What one port serves: the default, its override, and the endpoint's own enableQuic flag.
    /// </summary>
    private static IoxideProtocols ResolveProtocols(IoxideOptions options, ServerConfiguration config, ushort port)
    {
        var named = options.ProtocolsByPort.TryGetValue(port, out var configured);

        var protocols = named ? configured : options.Protocols;

        // HTTP/3 from the DEFAULT applies only where it can, so Protocols = All means "everything
        // each port supports" rather than an error about the plaintext one. Named per port it is
        // taken literally, and refused where the port has no certificate.
        if (!named && protocols.HasFlag(IoxideProtocols.Http3) && config.EndPoints.All(e => e.Port != port || e.Security is null))
        {
            protocols &= ~IoxideProtocols.Http3;
        }

        if (config.EndPoints.Any(e => e.Port == port && e.EnableQuic))
        {
            protocols |= IoxideProtocols.Http3;
        }

        if (protocols == 0)
        {
            throw new NotSupportedException($"Port {port} was given no protocols to serve.");
        }

        return protocols;
    }

    /// <summary>The protocols this port serves.</summary>
    private IoxideProtocols ProtocolsFor(ushort port)
        => _protocols.TryGetValue(port, out var protocols) ? protocols : IoxideProtocols.Http1;

    private string DescribeSettings()
    {
        var protocols = string.Join(" ", _protocols.OrderBy(p => p.Key).Select(p => $"{p.Key}:{Describe(p.Value)}"));

        return $"ioxide, {protocols}, TLS on {_secure.Count}"
               + (MutualTlsConfigured ? ", mTLS" : string.Empty)
               + $", DualStack: {_primary.DualStack}, Reactors: {_reactors?.Length ?? 0}";
    }

    private static string Describe(IoxideProtocols protocols)
    {
        var names = new List<string>(3);

        if (protocols.HasFlag(IoxideProtocols.Http1)) names.Add("h1");
        if (protocols.HasFlag(IoxideProtocols.Http2)) names.Add("h2");
        if (protocols.HasFlag(IoxideProtocols.Http3)) names.Add("h3");

        return string.Join("+", names);
    }

    public async ValueTask DisposeAsync()
    {
        Running = false;

        var reactors = _reactors;
        var threads = _threads;

        _reactors = null;
        _threads = null;

        if (reactors is null || threads is null)
        {
            return;
        }

        _logger.LogInformation("Stopping {Count} ioxide reactors ...", reactors.Length);

        // Stop, then join: each loop exits and Run() disposes its ring on the reactor thread, which
        // a single-issuer / DEFER_TASKRUN ring requires. Skipping it leaks a ring per host until
        // io_uring_setup runs out. Off the caller, so DisposeAsync stays non-blocking.
        await Task.Run(() =>
        {
            foreach (var reactor in reactors)
            {
                reactor.Stop();
            }

            foreach (var thread in threads)
            {
                thread.Join(TimeSpan.FromSeconds(5));
            }
        });

        DisposeQuic();

        _logger.LogInformation("Stopped ioxide reactors");
    }
}
