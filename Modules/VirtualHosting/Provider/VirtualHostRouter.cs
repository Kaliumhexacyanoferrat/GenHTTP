using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.VirtualHosting.Provider;

public sealed class VirtualHostRouter : IHandler
{

    #region Initialization

    public VirtualHostRouter(IHandler parent,
        Dictionary<string, IHandlerBuilder> hosts,
        IHandlerBuilder? defaultRoute)
    {
        Parent = parent;

        Hosts = hosts.ToDictionary(kv => kv.Key, kv => kv.Value.Build(this));
        DefaultRoute = defaultRoute?.Build(this);
    }

    #endregion

    #region Get-/Setters

    public IHandler Parent { get; }

    private Dictionary<string, IHandler> Hosts { get; }

    private IHandler? DefaultRoute { get; }

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
