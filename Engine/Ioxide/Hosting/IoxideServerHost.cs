using System.IO.Pipelines;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Shared.Hosting;
using GenHTTP.Engine.Shared.Infrastructure;

using ioxide;

namespace GenHTTP.Engine.Ioxide.Hosting;

public sealed class IoxideServerHost(Func<ServerConfig, ServerConfig>? configure = null, Action<Reactor>? onReactorStart = null, Func<TcpConnection, ValueTask<IDuplexPipe>>? connectionFactory = null, bool kernelTx = false, bool kernelRx = false, IoxideOptions? options = null) : ServerHost
{

    protected override IServer Build(ServerConfiguration config, IHandler handler)
        => new IoxideServer(config, handler, configure, onReactorStart, connectionFactory, kernelTx, kernelRx, options);
    
}
