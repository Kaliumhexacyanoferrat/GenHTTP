using System.IO.Pipelines;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Shared.Hosting;
using GenHTTP.Engine.Shared.Infrastructure;

using ioxide;

namespace GenHTTP.Engine.Ioxide.Hosting;

public sealed class IoxideServerHost(
    Action<Reactor>? onReactorStart = null,
    Func<TcpConnection, ValueTask<IDuplexPipe>>? connectionFactory = null,
    IoxideOptions? options = null) : ServerHost
{

    protected override IServer Build(ServerConfiguration config, IHandler handler)
        => new IoxideServer(config, handler, onReactorStart, connectionFactory, options);
    
}
