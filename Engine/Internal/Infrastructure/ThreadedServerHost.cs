using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Hosting;
using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Internal.Infrastructure;

internal sealed class ThreadedServerHost : ServerHost
{

    protected override IServer Build(ServerConfiguration config, IHandler handler) => new ThreadedServer(config, handler);

}
