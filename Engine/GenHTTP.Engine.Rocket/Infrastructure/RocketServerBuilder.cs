using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Shared.Infrastructure;
using URocket.Engine.Configs;

namespace GenHTTP.Engine.Rocket.Infrastructure;

public class RocketServerBuilder : ServerBuilder
{
    private readonly EngineOptions _engineOptions;
    
    public RocketServerBuilder(EngineOptions engineOptions)
    {
        _engineOptions = engineOptions;
    }
    
    protected override IServer Build(IServerCompanion? companion, ServerConfiguration config, IHandler handler)
    {
        return new RocketServer(companion, handler, config, _engineOptions);
    }
}