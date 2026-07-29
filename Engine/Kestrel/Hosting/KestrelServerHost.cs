using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Hosting;
using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Kestrel.Hosting;

public sealed class KestrelServerHost : ServerHost
{

    protected override IServer Build(ServerConfiguration config, IHandler handler) => new KestrelServerBridge(config, handler);

}
