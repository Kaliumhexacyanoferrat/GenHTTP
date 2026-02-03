using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Shared.Hosting;
using URocket.Engine.Configs;

namespace GenHTTP.Engine.Rocket;

public static class Host
{
    public static IServerHost Create(EngineOptions engineOptions)
    {
        return new ServerHost(Server.Create(engineOptions));
    }
}