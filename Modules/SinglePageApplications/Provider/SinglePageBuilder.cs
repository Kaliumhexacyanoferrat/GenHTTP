using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.SinglePageApplications.Provider;

public sealed class SinglePageBuilder : IHandlerBuilder<SinglePageBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private bool _serverSideRouting;

    private IResourceTree? _tree;

    #region Functionality

    public SinglePageBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public SinglePageBuilder Tree(IResourceTree tree)
    {
        _tree = tree;
        return this;
    }

    public SinglePageBuilder Tree(IBuilder<IResourceTree> tree) => Tree(tree.Build());

    /// <summary>
    /// If enabled, the server will serve the SPA index for
    /// every unknown route, which enables path based routing.
    /// </summary>
    public SinglePageBuilder ServerSideRouting()
    {
        _serverSideRouting = true;
        return this;
    }

    public IHandler Build()
    {
        var tree = _tree ?? throw new BuilderMissingPropertyException("tree");

        return Concerns.Chain(_concerns,  new SinglePageProvider( tree, _serverSideRouting));
    }

    #endregion

}
