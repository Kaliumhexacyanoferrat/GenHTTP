using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Rocket.Infrastructure;
using URocket.Engine.Configs;

namespace GenHTTP.Engine.Rocket;

public static class Server
{
    public static IServerBuilder Create(EngineOptions engineOptions)
    {
        return new RocketServerBuilder(engineOptions);
    }
}