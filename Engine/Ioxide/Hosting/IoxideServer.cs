using System.Diagnostics;
using System.IO.Pipelines;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide.Protocol;
using GenHTTP.Engine.Shared.Infrastructure;
using GenHTTP.Engine.Shared.Types;

using ioxide;
using ioxide.tls;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Ioxide.Hosting;

public sealed class IoxideServer : IServer
{
    private readonly ServerConfiguration _config;

    private readonly IoxideEndPoint _primary;

    private readonly Dictionary<ushort, IoxideEndPoint> _endPointByPort;

    private readonly Dictionary<ushort, SecurityConfiguration> _secure;

    private readonly ushort[] _extraPorts;

    private readonly Func<ServerConfig, ServerConfig>? _configure;

    private readonly Action<Reactor>? _onReactorStart;

    private readonly Func<TcpConnection, ValueTask<IDuplexPipe>>? _connectionFactory;

    private readonly bool _kernelTx;

    private readonly bool _kernelRx;

    private readonly ILogger _logger;

    private Thread[]? _threads;

    private Reactor[]? _reactors;

    public string Version { get; } = typeof(IoxideServer).Assembly.GetName().Version?.ToString() ?? "0.1";

    public bool Running { get; private set; }

    public bool Development => _config.DevelopmentMode;

    public IPropertyBag Properties { get; } = new PropertyBag();

    public ILoggerFactory Logging => _config.Logging;

    public IEndPointCollection EndPoints { get; }

    public IHandler Handler { get; }

    internal IoxideServer(ServerConfiguration config, IHandler handler, Func<ServerConfig, ServerConfig>? configure = null, Action<Reactor>? onReactorStart = null, Func<TcpConnection, ValueTask<IDuplexPipe>>? connectionFactory = null, bool kernelTx = false, bool kernelRx = false)
    {
        _config = config;
        Handler = handler;
        _configure = configure;
        _onReactorStart = onReactorStart;
        _connectionFactory = connectionFactory;
        _kernelTx = kernelTx;
        _kernelRx = kernelRx;

        _logger = config.Logging.CreateLogger<IoxideServer>();

        var mapped = config.EndPoints
                           .Select(e => new IoxideEndPoint(e.Address, e.Port, e.DualStack, e.Security != null))
                           .ToList();

        _primary = mapped[0];
        _endPointByPort = mapped.ToDictionary(e => e.Port);
        _extraPorts = mapped.Skip(1).Select(e => e.Port).ToArray();

        if (mapped.Any(e => e.DualStack != _primary.DualStack))
        {
            throw new NotSupportedException("The ioxide engine binds all endpoints with one dual-stack mode.");
        }

        // Certificates are resolved per reactor in OnStart, not here: the provider is queried for
        // its default (no-SNI) certificate then, and a port whose provider yields none is still
        // advertised as secure (so secure-upgrade redirects work) but serves no handshake.
        _secure = config.EndPoints
                        .Where(e => e.Security is not null)
                        .ToDictionary(e => e.Port, e => e.Security!);

        EndPoints = new IoxideEndPoints(mapped.Cast<IEndPoint>().ToList());
    }

    // The certificate for every secure port whose provider yields a default (no-SNI) certificate.
    // Providers that select by SNI (unsupported here) return none and are skipped - the port stays
    // advertised as secure but its handshakes are refused.
    private IEnumerable<KeyValuePair<ushort, TlsOptions>> ResolveTls()
    {
        foreach (var (port, security) in _secure)
        {
            if (security.CertificateValidator is not null)
            {
                throw new NotSupportedException("Client certificate validation is not supported by the ioxide engine.");
            }

            if (security.CertificateProvider.Provide(null) is not { } certificate)
            {
                _logger.LogWarning("No default certificate for secure port {Port}; handshakes there will be refused (SNI selection is unsupported).", port);
                continue;
            }

            yield return new(port, new TlsOptions
            {
                CertificatePem = certificate.ExportCertificatePem(),
                KeyPem = ExportKeyPem(certificate),
                KernelTx = _kernelTx,
                KernelRx = _kernelRx
            });
        }
    }

    private static string ExportKeyPem(X509Certificate2 certificate)
        => certificate.GetRSAPrivateKey()?.ExportPkcs8PrivateKeyPem()
           ?? certificate.GetECDsaPrivateKey()?.ExportPkcs8PrivateKeyPem()
           ?? throw new InvalidOperationException("The certificate carries no exportable RSA or ECDSA private key.");

    public async ValueTask StartAsync()
    {
        await PrepareHandlerAsync();

        Running = true;

        var cfg = new ServerConfig { ReactorCount = Environment.ProcessorCount };

        if (_configure is not null)
        {
            cfg = _configure(cfg);
        }

        // The endpoint bindings (.Port()/.Bind()) determine the listen ports and dual-stack mode, so
        // they always win over whatever the configuration hook may have set.
        cfg = cfg with
        {
            DualStack = _primary.DualStack,
            Tcp = (cfg.Tcp ?? new TcpOptions()) with
            {
                Port = _primary.Port,
                ExtraPorts = _extraPorts
            }
        };

        _threads = new Thread[cfg.ReactorCount];
        _reactors = new Reactor[cfg.ReactorCount];

        // Reactors bind their listeners on their own threads (inside Reactor.Run), so StartAsync must
        // not return until they're actually accepting - otherwise a client that connects immediately
        // (as the test host does) races the bind and gets "connection refused". OnStart fires right
        // after the listener is bound, so each reactor signals once it's up.
        var listening = new CountdownEvent(cfg.ReactorCount);

        for (var i = 0; i < _threads.Length; i++)
        {
            var reactor = new Reactor(i, cfg)
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
                TcpHandle = (_, c) => ConnectionDriver.HandleAsync(this, _endPointByPort[c.ListenerPort], c, _connectionFactory)
            };

            _reactors[i] = reactor;

            _threads[i] = new Thread(reactor.Run)
            {
                Name = $"ioxide-genhttp-{i}",
                IsBackground = true,
            };

            _threads[i].Start();
        }

        // Block (off the caller) until every reactor reports listening, so the server is accepting
        // before StartAsync returns. The timeout is a safety net for a reactor that fails to bind -
        // log and continue rather than hang the host forever.
        if (await Task.Run(() => listening.Wait(TimeSpan.FromSeconds(10))))
        {
            listening.Dispose();
        }
        else
        {
            _logger.LogWarning("Not all reactors reported listening within 10s; the server may not be fully accepting yet.");
        }

        _logger.LogInformation("Listening on {Address}:{Port} ({Settings})", _primary.Address, _primary.Port, DescribeSettings());
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

    private string DescribeSettings() => $"ioxide, {_endPointByPort.Count} endpoint(s), TLS on {_secure.Count}, DualStack: {_primary.DualStack}, Reactors: {_reactors?.Length ?? 0}";

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

        // Each reactor owns an io_uring ring on its own thread. Signal every reactor to stop, then join
        // the threads: each loop exits and Run() disposes its ring on the reactor thread (mandatory for a
        // single-issuer / DEFER_TASKRUN ring). Without this the rings leak for the lifetime of the process,
        // so a long-lived host - or a test run that spins up hundreds of hosts - eventually exhausts
        // io_uring_setup and crashes. Joining runs off the caller so DisposeAsync stays non-blocking.
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

        _logger.LogInformation("Stopped ioxide reactors");
    }

}
