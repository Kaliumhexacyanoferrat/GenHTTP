using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.StaticWebsites.Provider;

public class StaticWebsiteBuilder : IHandlerBuilder<StaticWebsiteBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private IResourceTree? _Tree;

    #region Functionality

    public StaticWebsiteBuilder Tree(IResourceTree tree)
    {
        _Tree = tree;
        return this;
    }

    public StaticWebsiteBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var tree = _Tree ?? throw new BuilderMissingPropertyException("tree");

        return Concerns.Chain(_Concerns,  new StaticWebsiteHandler( tree));
    }

    #endregion

}
