using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Infrastructure;

using Wired.IO.App;
using Wired.IO.Builder;
using Wired.IO.Http11;
using Wired.IO.Http11.Context;

namespace GenHTTP.Engine.WiredIO.Hosting;

public sealed class WiredServerBuilder : ServerBuilder
{
    private readonly Action<Builder<WiredHttp11, Http11Context>>? _ConfigurationHook;

    private readonly Action<WiredApp<Http11Context>>? _ApplicationHook;

    public WiredServerBuilder(Action<Builder<WiredHttp11, Http11Context>>? configurationHook, Action<WiredApp<Http11Context>>? applicationHook)
    {
        _ConfigurationHook = configurationHook;
        _ApplicationHook = applicationHook;
    }

    protected override IServer Build(IServerCompanion? companion, ServerConfiguration config, IHandler handler) => new WiredServer(companion, config, handler, _ConfigurationHook, _ApplicationHook);

}
