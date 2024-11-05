using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Internal.Hosting;

public class KestrelServerBuilder : ServerBuilder
{

    protected override IServer Build(IServerCompanion? companion, ServerConfiguration config, IHandler handler) => throw new NotImplementedException();

}
