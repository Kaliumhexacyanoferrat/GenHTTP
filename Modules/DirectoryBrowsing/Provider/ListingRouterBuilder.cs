using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider;

public sealed class ListingRouterBuilder : IHandlerBuilder<ListingRouterBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private IResourceTree? _tree;

    #region Functionality

    public ListingRouterBuilder Tree(IResourceTree tree)
    {
        _tree = tree;
        return this;
    }

    public ListingRouterBuilder Tree(IBuilder<IResourceTree> tree) => Tree(tree.Build());

    public ListingRouterBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var tree = _tree ?? throw new BuilderMissingPropertyException("tree");

        return Concerns.Chain(_concerns,  new ListingRouter( tree));
    }

    #endregion

}
