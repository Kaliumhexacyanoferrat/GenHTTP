using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Kestrel.Hosting;

public class KestrelServerBuilder : ServerBuilder
{

    protected override IServer Build(IServerCompanion? companion, ServerConfiguration config, IHandler handler) => new KestrelServer(companion, config, handler);

}
