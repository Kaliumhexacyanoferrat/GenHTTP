using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.SinglePageApplications.Provider;

public sealed class SinglePageBuilder : IHandlerBuilder<SinglePageBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private bool _ServerSideRouting;

    private IResourceTree? _Tree;

    #region Functionality

    public SinglePageBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public SinglePageBuilder Tree(IResourceTree tree)
    {
        _Tree = tree;
        return this;
    }

    public SinglePageBuilder Tree(IBuilder<IResourceTree> tree) => Tree(tree.Build());

    /// <summary>
    /// If enabled, the server will serve the SPA index for
    /// every unknown route, which enables path based routing.
    /// </summary>
    public SinglePageBuilder ServerSideRouting()
    {
        _ServerSideRouting = true;
        return this;
    }

    public IHandler Build()
    {
        var tree = _Tree ?? throw new BuilderMissingPropertyException("tree");

        return Concerns.Chain(_Concerns,  new SinglePageProvider( tree, _ServerSideRouting));
    }

    #endregion

}
