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
    private readonly ServerConfiguration _serverConfiguration;

    /// <summary>Every endpoint, in the order it was bound. The first one names the server.</summary>
    private readonly EndPoint[] _endPoints;

    /// <summary>
    /// One mode for the whole server, since that is all ioxide takes - see <see cref="MapEndPoints"/>,
    /// which refuses endpoints that disagree.
    /// </summary>
    private readonly bool _dualStack;

    private readonly Action<Reactor>? _onReactorStart;

    private readonly EngineOptions _engineOptions;

    private readonly ILogger _logger;

    private Thread[]? _threads;

    private Reactor[]? _reactors;

#region Get-/Setters
    
    public string Version { get; } = typeof(Server).Assembly.GetName().Version?.ToString() ?? "0.1";

    public bool Running { get; private set; }

    public bool Development => _serverConfiguration.DevelopmentMode;

    public IPropertyBag Properties { get; } = new PropertyBag();

    public ILoggerFactory Logging => _serverConfiguration.Logging;

    public IEndPointCollection EndPoints { get; }

    public IHandler Handler { get; }
    
#endregion

#region Constructors
    
    internal Server(
        ServerConfiguration serverConfiguration, 
        IHandler handler,
        Action<Reactor>? onReactorStart = null,
        EngineOptions? options = null)
    {
        _serverConfiguration = serverConfiguration;
        Handler = handler;
        _onReactorStart = onReactorStart;
        _engineOptions = options ?? EngineOptions.Default;

        _logger = serverConfiguration.Logging.CreateLogger<Server>();

        _endPoints = MapEndPoints(serverConfiguration, _engineOptions);
        _dualStack = _endPoints[0].DualStack;

        // Which endpoints want which listener, settled here so StartAsync only has to act on it.
        // Both read _endPoints, which each endpoint's own protocols now come with.
        _tcpPorts = ResolveTcpPorts();
        _quicEndPoint = ResolveQuicEndPoint();

        EndPoints = new EndPointCollection(_endPoints);
    }
    
#endregion

    public async ValueTask StartAsync()
    {
        await PrepareHandlerAsync();

        Running = true;
        
        var serverConfig = BuildServerConfig();

        if (_tcpPorts.Length > 0)
        {
            serverConfig = WithTcp(serverConfig);
        }

        if (_quicEndPoint is not null)
        {
            serverConfig = WithQuic(serverConfig);
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

                    if (SecureEndPoints.Any())
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
                {
                    var endPoint = EndPointFor(tcpConnection.ListenerPort);

                    return ConnectionDriver.HandleAsync(this, endPoint, tcpConnection, endPoint.Protocols);
                },
                
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
            _logger.LogInformation("Listening on {Address}:{Port} ({Settings})", _endPoints[0].Address, _endPoints[0].Port, DescribeSettings());
        }
    }

    /// <summary>
    /// GenHTTP's endpoints as the engine's own, resolving what each one serves, and where the two
    /// things the engine cannot express are refused: a port bound twice, and endpoints asking for
    /// different dual-stack modes.
    /// </summary>
    /// <remarks>
    /// GenHTTP takes dual-stack per endpoint, on <c>Bind</c>; ioxide takes one flag for the whole
    /// server, deciding whether listeners are IPv6 on :: with V6ONLY off or plain IPv4 on 0.0.0.0.
    /// The engine can only honour one, and honours the first endpoint's - so endpoints that
    /// disagree are refused here rather than silently served the first one's mode.
    /// </remarks>
    private static EndPoint[] MapEndPoints(ServerConfiguration config, EngineOptions options)
    {
        var mapped = config.EndPoints.Select(e => Map(e, options)).ToArray();

        // A connection carries only the port it arrived on, so that port has to name one endpoint.
        // Checked explicitly because nothing else keys them by port any more.
        if (mapped.GroupBy(e => e.Port).FirstOrDefault(g => g.Count() > 1) is { } duplicate)
        {
            throw new NotSupportedException(
                $"Port {duplicate.Key} was bound {duplicate.Count()} times. A connection is matched to its endpoint "
                + "by the port it arrived on, so each port carries one endpoint.");
        }

        var dualStack = mapped[0].DualStack;

        // No disagreement between DualStack capability among endpoints is supported as of today
        // To support that user can simply have two different IServerHost instances running.
        if (mapped.Any(e => e.DualStack != dualStack))
        {
            var disagreeing = mapped.Where(e => e.DualStack != dualStack).Select(e => e.Port);

            throw new NotSupportedException(
                $"The ioxide engine binds every endpoint with one dual-stack mode, taken from the first one bound "
                + $"(port {mapped[0].Port}, DualStack = {dualStack}). These ask for the other: {string.Join(", ", disagreeing)}.");
        }

        return mapped;
    }

    /// <summary>
    /// The engine-wide configuration, straight from the options. The listeners are added on top by
    /// <c>WithTcp</c> and <c>WithQuic</c>, which take their ports from the endpoint bindings.
    /// </summary>
    private ServerConfig BuildServerConfig() => new()
    {
        ReactorCount = _engineOptions.Reactor.ReactorCount,
        RingEntries = _engineOptions.Reactor.RingEntries,
        RecvBufferSize = _engineOptions.Reactor.RecvBufferSize,
        RecvSlots = _engineOptions.Reactor.RecvSlots,
        Incremental = _engineOptions.Reactor.Incremental,

        // Server-wide rather than per transport: it applies to the TCP listener and the UDP socket
        // alike, which is why the engine binds every endpoint with one mode.
        DualStack = _dualStack,

        // No listeners yet - WithTcp and WithQuic add the ones the bindings ask for. Explicitly
        // null because ioxide's own default is a live listener on 8080, which an HTTP/3-only server
        // would otherwise inherit and bind for a protocol it does not serve.
        Tcp = null,
    };

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
    /// One binding as the engine's own endpoint. A certificate makes it a <see cref="SecureEndPoint"/>,
    /// which is what carries the TLS settings; without one it is cleartext and has none to carry.
    /// </summary>
    private static EndPoint Map(EndPointConfiguration endPoint, EngineOptions options)
    {
        var protocols = ResolveProtocols(options, endPoint);

        return endPoint.Security is { } security
            ? new SecureEndPoint(endPoint.Address, endPoint.Port, endPoint.DualStack, protocols, security, options.MutualTls)
            : new InsecureEndPoint(endPoint.Address, endPoint.Port, endPoint.DualStack, protocols);
    }

    /// <summary>
    /// What one endpoint serves: the default, its port's override, and its own enableQuic flag.
    /// </summary>
    private static Protocols ResolveProtocols(EngineOptions options, EndPointConfiguration endPoint)
    {
        var named = options.ProtocolsByPort.TryGetValue(endPoint.Port, out var configured);

        var protocols = named ? configured : options.Protocols;

        // HTTP/3 from the DEFAULT applies only where it can, so Protocols = All means "everything
        // each port supports" rather than an error about the plaintext one. Named per port it is
        // taken literally, and refused where the port has no certificate.
        if (!named && protocols.HasFlag(Protocols.Http3) && endPoint.Security is null)
        {
            protocols &= ~Protocols.Http3;
        }

        if (endPoint.EnableQuic)
        {
            protocols |= Protocols.Http3;
        }

        if (protocols == 0)
        {
            throw new NotSupportedException($"Port {endPoint.Port} was given no protocols to serve.");
        }

        return protocols;
    }

    /// <summary>
    /// The endpoint a connection arrived on, matched by the port its listener bound. A scan rather
    /// than a table: a server binds a handful of endpoints, so the array is the whole truth and
    /// there is no second copy of it to keep in step.
    /// </summary>
    private EndPoint EndPointFor(ushort port)
    {
        foreach (var endPoint in _endPoints)
        {
            if (endPoint.Port == port)
            {
                return endPoint;
            }
        }

        throw new InvalidOperationException($"A connection arrived on port {port}, which no endpoint is bound to.");
    }

    private string DescribeSettings()
    {
        var protocols = string.Join(" ", _endPoints.OrderBy(e => e.Port).Select(e => $"{e.Port}:{Describe(e.Protocols)}"));

        return $"ioxide, {protocols}, TLS on {SecureEndPoints.Count()}"
               + (MutualTlsConfigured ? ", mTLS" : string.Empty)
               + $", DualStack: {_dualStack}, Reactors: {_reactors?.Length ?? 0}";
    }

    private static string Describe(Protocols protocols)
    {
        var names = new List<string>(3);

        if (protocols.HasFlag(Protocols.Http1)) names.Add("h1");
        if (protocols.HasFlag(Protocols.Http2)) names.Add("h2");
        if (protocols.HasFlag(Protocols.Http3)) names.Add("h3");

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
