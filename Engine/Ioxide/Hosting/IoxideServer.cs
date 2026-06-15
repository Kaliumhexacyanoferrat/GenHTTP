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

    private Thread[]? _threads;

    public string Version { get; } = typeof(IoxideServer).Assembly.GetName().Version?.ToString() ?? "0.1";

    public bool Running { get; private set; }

    public bool Development => _config.DevelopmentMode;

    public IPropertyBag Properties { get; } = new PropertyBag();

    public IEndPointCollection EndPoints { get; }

    public IServerCompanion? Companion { get; }

    public IHandler Handler { get; }

    internal IoxideServer(IServerCompanion? companion, ServerConfiguration config, IHandler handler, Func<ServerConfig, ServerConfig>? configure = null)
    {
        Companion = companion;
        _config = config;
        Handler = handler;
        _configure = configure;

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

        for (var i = 0; i < _threads.Length; i++)
        {
            var reactor = new Reactor(i, cfg)
            {
                Handle = (_, c) => ConnectionDriver.HandleAsync(this, _endPoint, c),
            };

            _threads[i] = new Thread(reactor.Run)
            {
                Name = $"ioxide-genhttp-{i}",
                IsBackground = true,
            };

            _threads[i].Start();
        }
    }

    public ValueTask DisposeAsync()
    {
        Running = false; // spike: reactor threads are background; no graceful drain
        
        return ValueTask.CompletedTask;
    }
}
