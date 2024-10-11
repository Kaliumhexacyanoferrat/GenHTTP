using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.StaticWebsites.Provider;

public class StaticWebsiteBuilder : IHandlerBuilder<StaticWebsiteBuilder>
{
    private IResourceTree? _Tree;

    private readonly List<IConcernBuilder> _Concerns = new();

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

    public IHandler Build(IHandler parent)
    {
            var tree = _Tree ?? throw new BuilderMissingPropertyException("tree");

            return Concerns.Chain(parent, _Concerns, (p) => new StaticWebsiteHandler(p, tree));
        }

    #endregion

}
