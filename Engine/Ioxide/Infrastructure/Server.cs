using System.Diagnostics;
using System.IO.Pipelines;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

using GenHTTP.Engine.Shared.Infrastructure;
using GenHTTP.Engine.Shared.Types;

using ioxide;
using ioxide.tls;

using Microsoft.Extensions.Logging;
using GenHTTP.Engine.Ioxide.Protocol.Drivers.Quic;
using GenHTTP.Engine.Ioxide.Protocol.Drivers.Tcp;

namespace GenHTTP.Engine.Ioxide.Infrastructure;

/// <summary>The server: maps the bindings, runs a reactor per thread, and stops them again.</summary>
public sealed partial class Server : IServer
{
    private readonly ServerConfiguration _serverConfiguration;

    private readonly EndPoint[] _endPoints;

    private readonly bool _dualStack;

    private readonly Action<Reactor>? _onReactorStart;

    private readonly EngineOptions _engineOptions;

    private readonly ILogger _logger;

    private Thread[]? _threads;

    private Reactor[]? _reactors;

    private TcpTlsRegistry?[]? _tcpTlsRegistries;

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
    
    // Settles everything the bindings decide, so StartAsync only has to act on it.
    internal Server(
        ServerConfiguration serverConfiguration, 
        IHandler handler,
        Action<Reactor>? onReactorStart = null,
        EngineOptions? options = null)
    {
        Handler = handler;
        
        _serverConfiguration = serverConfiguration;
        _onReactorStart = onReactorStart;
        _engineOptions = options ?? EngineOptions.Default;
        _logger = serverConfiguration.Logging.CreateLogger<Server>();
        _endPoints = MapEndPoints(serverConfiguration);
        _dualStack = _endPoints[0].DualStack;

        _tcpPorts = ResolveTcpPorts();
        _quicEndPoint = ResolveQuicEndPoint();

        EndPoints = new EndPointCollection(_endPoints);
    }
    
#endregion

    // Starts a reactor per thread and waits until every one of them is listening.
    public async ValueTask StartAsync()
    {
        await PrepareHandlerAsync();

        Running = true;
        
        var serverConfig = BuildServerConfig();

        bool secureTcp = SecureTcpEndPoints.Any();

        KeyValuePair<ushort, TlsOptions>[] portsTlsOptions = secureTcp ? ResolveTls().ToArray() : [];

        WarnAboutValidatorGaps();

        _threads = new Thread[serverConfig.ReactorCount];
        _reactors = new Reactor[serverConfig.ReactorCount];
        _tcpTlsRegistries = secureTcp ? new TcpTlsRegistry?[serverConfig.ReactorCount] : null;

        CountdownEvent listening = new CountdownEvent(serverConfig.ReactorCount);

        for (var i = 0; i < _threads.Length; i++)
        {
            // OnStart runs on the reactor's thread, long after the loop variable has moved on.
            int capturedIndex = i;

            Reactor reactor = new Reactor(i, serverConfig)
            {
                OnStart = _reactor_ =>
                {
                    IoxideReactor.Bind(_reactor_);

                    if (secureTcp)
                    {
                        StartTlsServices(_reactor_, capturedIndex, portsTlsOptions);
                    }

                    _onReactorStart?.Invoke(_reactor_);

                    listening.Signal();
                },
                TcpHandle = (_, tcpConnection) =>
                {
                    EndPoint endPoint = EndPointFor(tcpConnection.ListenerPort);

                    return TcpDriver.HandleAsync(this, endPoint, tcpConnection, endPoint.Protocols);
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

    // Turns the bindings into endpoints, refusing the combinations this engine cannot serve.
    private static EndPoint[] MapEndPoints(ServerConfiguration config)
    {
        var mapped = config.EndPoints.Select(Map).ToArray();

        if (mapped.GroupBy(e => e.Port).FirstOrDefault(g => g.Count() > 1) is { } duplicate)
        {
            throw new NotSupportedException(
                $"Port {duplicate.Key} was bound {duplicate.Count()} times. A connection is matched to its endpoint "
                + "by the port it arrived on, so each port carries one endpoint.");
        }

        foreach (var endPoint in mapped.OfType<SecureEndPoint>())
        {
            if (endPoint.RequireClientCertificate && endPoint.ClientCaPath is null && endPoint.ClientCaPem is null)
            {
                throw new NotSupportedException(
                    $"Port {endPoint.Port} requires a client certificate but names nothing to validate one against. "
                    + $"Pass an {nameof(IMutualTlsValidator)} to Bind with ClientCaPath or ClientCaPem set, or leave "
                    + "RequireCertificate false to let the connection in and decide in the handler.");
            }
        }

        var dualStack = mapped[0].DualStack;

        if (mapped.Any(e => e.DualStack != dualStack))
        {
            var disagreeing = mapped.Where(e => e.DualStack != dualStack).Select(e => e.Port);

            throw new NotSupportedException(
                $"The ioxide engine binds every endpoint with one dual-stack mode, taken from the first one bound "
                + $"(port {mapped[0].Port}, DualStack = {dualStack}). These ask for the other: {string.Join(", ", disagreeing)}.");
        }

        return mapped;
    }

    // The engine-wide configuration, plus whichever listeners the bindings call for.
    private ServerConfig BuildServerConfig()
    {
        var serverConfig = new ServerConfig
        {
            ReactorCount = _engineOptions.Reactor.ReactorCount,
            RingEntries = _engineOptions.Reactor.RingEntries,
            RecvBufferSize = _engineOptions.Reactor.RecvBufferSize,
            RecvSlots = _engineOptions.Reactor.RecvSlots,
            Incremental = _engineOptions.Reactor.Incremental,

            DualStack = _dualStack,

            // Explicitly null: ioxide's own default is a live listener on 8080, which an
            // HTTP/3-only server would otherwise inherit.
            Tcp = null,
        };
        
        if (_tcpPorts.Length > 0)
        {
            serverConfig = WithTcp(serverConfig);
        }

        if (_quicEndPoint is not null)
        {
            serverConfig = WithQuic(serverConfig);
        }
        
        return serverConfig;
    }

    // Warms the handler chain before any connection can reach it; logs rather than throws.
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
    
    // One binding as an endpoint - a certificate is what makes it a secure one. The binding names
    // the protocols; the shared validator settles what this configuration can actually serve
    // (dropping HTTP/3 where there is no certificate, refusing a port left with nothing).
    private static EndPoint Map(EndPointConfiguration endPoint)
    {
        var protocols = Protocols.Validate(endPoint);

        return endPoint.Security is { } security
            ? new SecureEndPoint(endPoint.Address, endPoint.Port, endPoint.DualStack, protocols, security)
            : new InsecureEndPoint(endPoint.Address, endPoint.Port, endPoint.DualStack, protocols);
    }

    // The endpoint a connection arrived on. A scan, since a server binds a handful.
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

    // The one-line summary logged once the server is up.
    private string DescribeSettings()
    {
        var protocols = string.Join(" ", _endPoints.OrderBy(e => e.Port).Select(e => $"{e.Port}:{Describe(e.Protocols)}"));

        return $"ioxide, {protocols}, TLS on {SecureEndPoints.Count()}"
               + (MutualTlsConfigured ? ", mTLS" : string.Empty)
               + $", DualStack: {_dualStack}, Reactors: {_reactors?.Length ?? 0}";
    }

    // Protocol flags as the ALPN-ish names an operator recognises.
    private static string Describe(HttpProtocols httpProtocols)
    {
        var names = new List<string>(3);

        if (httpProtocols.HasFlag(HttpProtocols.Http1)) names.Add("h1");
        if (httpProtocols.HasFlag(HttpProtocols.Http2)) names.Add("h2");
        if (httpProtocols.HasFlag(HttpProtocols.Http3)) names.Add("h3");

        return string.Join("+", names);
    }

    // Stops every reactor and joins its thread, so no ring outlives the server.
    public async ValueTask DisposeAsync()
    {
        Running = false;

        var reactors = _reactors;
        var threads = _threads;

        _reactors = null;
        _threads = null;
        _tcpTlsRegistries = null;

        if (reactors is null || threads is null)
        {
            return;
        }

        _logger.LogInformation("Stopping {Count} ioxide reactors ...", reactors.Length);

        await Task.Run(() =>
        {
            // Stop, then join: a single-issuer / DEFER_TASKRUN ring must be disposed on its own
            // thread, and skipping the join leaks a ring per host.
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
