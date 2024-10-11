using GenHTTP.Api.Content;

namespace GenHTTP.Modules.VirtualHosting.Provider;

public sealed class VirtualHostRouterBuilder : IHandlerBuilder<VirtualHostRouterBuilder>
{
    private readonly Dictionary<string, IHandlerBuilder> _Hosts = [];

    private readonly List<IConcernBuilder> _Concerns = [];

    private IHandlerBuilder? _DefaultRoute;

    #region Functionality

    public VirtualHostRouterBuilder Add(string host, IHandlerBuilder handler)
    {
        if (!_Hosts.TryAdd(host, handler))
        {
            throw new InvalidOperationException("A host with this name has already been added");
        }

        return this;
    }

    public VirtualHostRouterBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public VirtualHostRouterBuilder Default(IHandlerBuilder handler)
    {
        _DefaultRoute = handler;
        return this;
    }

    public IHandler Build(IHandler parent)
    {
        return Concerns.Chain(parent, _Concerns, p => new VirtualHostRouter(p, _Hosts, _DefaultRoute));
    }

    #endregion

}
