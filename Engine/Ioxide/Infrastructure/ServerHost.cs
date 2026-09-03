using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Shared.Infrastructure;

using ioxide;

namespace GenHTTP.Engine.Ioxide.Infrastructure;

public sealed class ServerHost(
    Action<Reactor>? onReactorStart = null,
    EngineOptions? options = null) : Shared.Hosting.ServerHost
{
    protected override IServer Build(ServerConfiguration config, IHandler handler)
        => new Server(config, handler, onReactorStart, options);
}
