using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Infrastructure;

using Microsoft.AspNetCore.Builder;

namespace GenHTTP.Engine.Kestrel.Hosting;

public sealed class KestrelServerBuilder : ServerBuilder
{
    private readonly Action<WebApplicationBuilder>? _configurationHook;

    private readonly Action<WebApplication>? _applicationHook;

    public KestrelServerBuilder(Action<WebApplicationBuilder>? configurationHook, Action<WebApplication>? applicationHook)
    {
        _configurationHook = configurationHook;
        _applicationHook = applicationHook;
    }

    protected override IServer Build(IServerCompanion? companion, ServerConfiguration config, IHandler handler) => new KestrelServer(companion, config, handler, _configurationHook, _applicationHook);

}
