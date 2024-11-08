using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Infrastructure;

using Microsoft.AspNetCore.Builder;

namespace GenHTTP.Engine.Kestrel.Hosting;

public sealed class KestrelServerBuilder : ServerBuilder
{
    private readonly Action<WebApplicationBuilder>? _ConfigurationHook;

    private readonly Action<WebApplication>? _ApplicationHook;

    public KestrelServerBuilder(Action<WebApplicationBuilder>? configurationHook, Action<WebApplication>? applicationHook)
    {
        _ConfigurationHook = configurationHook;
        _ApplicationHook = applicationHook;
    }

    protected override IServer Build(IServerCompanion? companion, ServerConfiguration config, IHandler handler) => new KestrelServer(companion, config, handler, _ConfigurationHook, _ApplicationHook);

}
