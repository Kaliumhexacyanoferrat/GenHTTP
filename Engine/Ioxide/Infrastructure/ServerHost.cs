using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Shared.Infrastructure;

using ioxide;

namespace GenHTTP.Engine.Ioxide.Infrastructure;

/// <summary>Builds an ioxide server once the shared hosting layer has settled the configuration.</summary>
public sealed class ServerHost(
    Action<Reactor>? onReactorStart = null,
    EngineOptions? options = null) : Shared.Hosting.ServerHost
{
    // Hands the shared hosting layer an ioxide server once the bindings are settled.
    protected override IServer Build(ServerConfiguration config, IHandler handler)
        => new Server(config, handler, onReactorStart, options);
}
