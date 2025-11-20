using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.IO.Providers;

public sealed class ResourceHandlerBuilder : IHandlerBuilder<ResourceHandlerBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private IResourceTree? _tree;

    #region Functionality

    public ResourceHandlerBuilder Tree(IBuilder<IResourceTree> tree) => Tree(tree.Build());

    public ResourceHandlerBuilder Tree(IResourceTree tree)
    {
        _tree = tree;
        return this;
    }

    public ResourceHandlerBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var tree = _tree ?? throw new BuilderMissingPropertyException("tree");

        return Concerns.Chain(_concerns,  new ResourceHandler( tree));
    }

    #endregion

}
