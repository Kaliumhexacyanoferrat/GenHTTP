using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Hosting;
using GenHTTP.Engine.Shared.Infrastructure;
using Microsoft.AspNetCore.Builder;

namespace GenHTTP.Engine.Kestrel.Hosting;

public sealed class KestrelServerHost(Action<WebApplicationBuilder>? configHook, Action<WebApplication>? appHook) : ServerHost
{

    protected override IServer Build(ServerConfiguration config, IHandler handler) => new KestrelServerBridge(config, handler, configHook, appHook);

}
