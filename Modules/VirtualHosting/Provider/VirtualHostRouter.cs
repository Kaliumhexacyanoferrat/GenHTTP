using System.Collections.Frozen;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Modules.VirtualHosting.Provider;

public sealed class VirtualHostRouter : IHandler
{
    private static readonly ReadOnlyMemory<byte> HostHeader = "Host"u8.ToArray();

    #region Get-/Setters

    private FrozenDictionary<PathSegment, IHandler> Hosts { get; }

    private IHandler? DefaultRoute { get; }

    #endregion

    #region Initialization

    public VirtualHostRouter(Dictionary<PathSegment, IHandlerBuilder> hosts, IHandlerBuilder? defaultRoute)
    {
        Hosts = hosts.ToFrozenDictionary(kv => kv.Key, kv => kv.Value.Build());
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
        var host = GetHostWithoutPort(request);

        // try to find a regular route
        if (host is not null)
        {
            if (Hosts.TryGetValue(new(host.Value), out var handler))
            {
                return handler;
            }
        }

        // route by default
        return DefaultRoute;
    }

    private static ReadOnlyMemory<byte>? GetHostWithoutPort(IRequest request)
    {
        var host = request.Raw.Header.Headers.GetEntry(HostHeader);

        if (host == null)
        {
            return null;
        }

        var hostSpan = host.Value.Span;
        var colonIndex = hostSpan.IndexOf((byte)':');

        return colonIndex >= 0 ? host.Value[..colonIndex] : host.Value;
    }

    #endregion

}
