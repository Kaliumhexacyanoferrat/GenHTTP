using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide.Infrastructure;
using ioxide;

namespace GenHTTP.Engine.Ioxide;

public static class Host
{
    public static IServerHost Create(Action<Reactor>? onReactorStart = null, EngineOptions? options = null)
        => new ServerHost(onReactorStart, options);
}
