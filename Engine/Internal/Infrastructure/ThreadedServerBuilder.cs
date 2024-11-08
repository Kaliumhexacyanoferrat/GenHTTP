using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Internal.Infrastructure;

internal sealed class ThreadedServerBuilder : ServerBuilder
{

    protected override IServer Build(IServerCompanion? companion, ServerConfiguration config, IHandler handler) => new ThreadedServer(companion, config, handler);

}
