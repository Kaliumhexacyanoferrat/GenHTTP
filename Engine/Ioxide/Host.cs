using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide.Infrastructure;
using ioxide;

namespace GenHTTP.Engine.Ioxide;

/// <summary>Entry point for a server host running on the ioxide engine.</summary>
public static class Host
{
    // Creates a server host that runs on the ioxide engine.
    public static IServerHost Create(Action<Reactor>? onReactorStart = null, EngineOptions? options = null)
        => new ServerHost(onReactorStart, options);
}
