using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider;

public sealed class ListingRouterBuilder : IHandlerBuilder<ListingRouterBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private IResourceTree? _Tree;

    #region Functionality

    public ListingRouterBuilder Tree(IResourceTree tree)
    {
        _Tree = tree;
        return this;
    }

    public ListingRouterBuilder Tree(IBuilder<IResourceTree> tree) => Tree(tree.Build());

    public ListingRouterBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var tree = _Tree ?? throw new BuilderMissingPropertyException("tree");

        return Concerns.Chain(_Concerns,  new ListingRouter( tree));
    }

    #endregion

}
