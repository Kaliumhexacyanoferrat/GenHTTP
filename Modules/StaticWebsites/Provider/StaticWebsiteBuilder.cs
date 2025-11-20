using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.StaticWebsites.Provider;

public class StaticWebsiteBuilder : IHandlerBuilder<StaticWebsiteBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private IResourceTree? _tree;

    #region Functionality

    public StaticWebsiteBuilder Tree(IResourceTree tree)
    {
        _tree = tree;
        return this;
    }

    public StaticWebsiteBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var tree = _tree ?? throw new BuilderMissingPropertyException("tree");

        return Concerns.Chain(_concerns,  new StaticWebsiteHandler( tree));
    }

    #endregion

}
