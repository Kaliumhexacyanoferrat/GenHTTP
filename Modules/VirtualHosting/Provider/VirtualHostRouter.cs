using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.VirtualHosting.Provider;

public sealed class VirtualHostRouter : IHandler
{

    #region Get-/Setters

    private Dictionary<string, IHandler> Hosts { get; }

    private IHandler? DefaultRoute { get; }

    #endregion

    #region Initialization

    public VirtualHostRouter(Dictionary<string, IHandlerBuilder> hosts, IHandlerBuilder? defaultRoute)
    {
        Hosts = hosts.ToDictionary(kv => kv.Key, kv => kv.Value.Build());
        DefaultRoute = defaultRoute?.Build();
    }

    #endregion

    #region Functionality

    public async ValueTask PrepareAsync()
    {
        foreach (var host in Hosts.Values)
        {
            await host.PrepareAsync();
        }

        if (DefaultRoute != null)
        {
            await DefaultRoute.PrepareAsync();
        }
    }

    public ValueTask<IResponse?> HandleAsync(IRequest request) => GetHandler(request)?.HandleAsync(request) ?? new ValueTask<IResponse?>();

    private IHandler? GetHandler(IRequest request)
    {
        var host = request.HostWithoutPort();

        // try to find a regular route
        if (host is not null)
        {
            if (Hosts.TryGetValue(host, out var handler))
            {
                return handler;
            }
        }

        // route by default
        return DefaultRoute;
    }

    #endregion

}
