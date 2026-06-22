using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Hosting;
using GenHTTP.Engine.Shared.Infrastructure;

using Microsoft.AspNetCore.Builder;

namespace GenHTTP.Engine.Kestrel.Hosting;

public sealed class KestrelServerHost : ServerHost
{
    private readonly Action<WebApplicationBuilder>? _configurationHook;

    private readonly Action<WebApplication>? _applicationHook;

    public KestrelServerHost(Action<WebApplicationBuilder>? configurationHook, Action<WebApplication>? applicationHook)
    {
        _configurationHook = configurationHook;
        _applicationHook = applicationHook;
    }

    protected override IServer Build(ServerConfiguration config, IHandler handler) => new KestrelServer(null, config, handler, _configurationHook, _applicationHook);

}
