using System.IO.Pipelines;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide.Protocol;
using GenHTTP.Engine.Shared.Infrastructure;
using GenHTTP.Engine.Shared.Types;

using ioxide;

namespace GenHTTP.Engine.Ioxide.Hosting;

public sealed class IoxideServer : IServer
{
    private readonly ServerConfiguration _config;

    private readonly IoxideEndPoint _endPoint;

    private readonly Func<ServerConfig, ServerConfig>? _configure;

    private readonly Action<Reactor>? _onReactorStart;

    private readonly Func<Connection, ValueTask<IDuplexPipe>>? _connectionFactory;

    private Thread[]? _threads;

    private Reactor[]? _reactors;

    public string Version { get; } = typeof(IoxideServer).Assembly.GetName().Version?.ToString() ?? "0.1";

    public bool Running { get; private set; }

    public bool Development => _config.DevelopmentMode;

    public IPropertyBag Properties { get; } = new PropertyBag();

    public IEndPointCollection EndPoints { get; }

    public IServerCompanion? Companion { get; }

    public IHandler Handler { get; }

    internal IoxideServer(IServerCompanion? companion, ServerConfiguration config, IHandler handler, Func<ServerConfig, ServerConfig>? configure = null, Action<Reactor>? onReactorStart = null, Func<Connection, ValueTask<IDuplexPipe>>? connectionFactory = null)
    {
        Companion = companion;
        _config = config;
        Handler = handler;
        _configure = configure;
        _onReactorStart = onReactorStart;
        _connectionFactory = connectionFactory;

        var ep = config.EndPoints.First(); // spike: first endpoint only

        _endPoint = new IoxideEndPoint(ep.Address, ep.Port, ep.DualStack, ep.Security != null);
        EndPoints = new IoxideEndPoints([_endPoint]);
    }

    public async ValueTask StartAsync()
    {
        // Initialise the handler chain (routing, reflection, websockets, ...) before serving;
        // GenHTTP handlers throw "Handler is not prepared yet" otherwise.
        await Handler.PrepareAsync(this);

        Running = true;

        var cfg = new ServerConfig { ReactorCount = Environment.ProcessorCount };

        if (_configure is not null)
        {
            cfg = _configure(cfg);
        }

        // The endpoint binding (.Port()/.Bind()) determines the listen port, so it
        // always wins over whatever the configuration hook may have set.
        cfg = cfg with
        {
            Port = _endPoint.Port
        };

        _threads = new Thread[cfg.ReactorCount];
        _reactors = new Reactor[cfg.ReactorCount];

        for (var i = 0; i < _threads.Length; i++)
        {
            var reactor = new Reactor(i, cfg)
            {
                // Runs once on the reactor's own thread before it serves: bind the reactor into the
                // [ThreadStatic] seam so handler code can resolve per-reactor services, then let the
                // host register those services (e.g. PgPool.Start(r, ...)) on this reactor's ring.
                OnStart = r =>
                {
                    IoxideReactor.Bind(r);
                    _onReactorStart?.Invoke(r);
                },
                Handle = (_, c) => ConnectionDriver.HandleAsync(this, _endPoint, c, _connectionFactory),
            };

            _reactors[i] = reactor;

            _threads[i] = new Thread(reactor.Run)
            {
                Name = $"ioxide-genhttp-{i}",
                IsBackground = true,
            };

            _threads[i].Start();
        }
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
    }
}
