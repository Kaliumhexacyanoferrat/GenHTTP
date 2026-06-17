using System.IO.Pipelines;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Infrastructure;

using ioxide;

namespace GenHTTP.Engine.Ioxide.Hosting;

public sealed class IoxideServerBuilder(Func<ServerConfig, ServerConfig>? configure = null, Action<Reactor>? onReactorStart = null, Func<Connection, ValueTask<IDuplexPipe>>? connectionFactory = null) : ServerBuilder
{
    protected override IServer Build(IServerCompanion? companion, ServerConfiguration config, IHandler handler)
        => new IoxideServer(companion, config, handler, configure, onReactorStart, connectionFactory);
}
