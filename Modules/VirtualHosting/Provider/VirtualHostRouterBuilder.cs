using GenHTTP.Api.Content;

namespace GenHTTP.Modules.VirtualHosting.Provider;

public sealed class VirtualHostRouterBuilder : IHandlerBuilder<VirtualHostRouterBuilder>
{

    private readonly List<IConcernBuilder> _concerns = [];
    private readonly Dictionary<string, IHandlerBuilder> _hosts = [];

    private IHandlerBuilder? _defaultRoute;

    #region Functionality

    public VirtualHostRouterBuilder Add(string host, IHandlerBuilder handler)
    {
        if (!_hosts.TryAdd(host, handler))
        {
            throw new InvalidOperationException("A host with this name has already been added");
        }

        return this;
    }

    public VirtualHostRouterBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public VirtualHostRouterBuilder Default(IHandlerBuilder handler)
    {
        _defaultRoute = handler;
        return this;
    }

    public IHandler Build()
    {
        return Concerns.Chain(_concerns,  new VirtualHostRouter( _hosts, _defaultRoute));
    }

    #endregion

}
