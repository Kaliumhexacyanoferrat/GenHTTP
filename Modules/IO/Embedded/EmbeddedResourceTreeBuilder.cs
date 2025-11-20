using System.Reflection;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.IO.Embedded;

public sealed class EmbeddedResourceTreeBuilder : IBuilder<IResourceTree>
{
    private string? _root;

    private Assembly? _source;

    #region Functionality

    public EmbeddedResourceTreeBuilder Source(Assembly source)
    {
        _source = source;
        return this;
    }

    public EmbeddedResourceTreeBuilder Root(string root)
    {
        _root = root;
        return this;
    }

    public IResourceTree Build()
    {
        var source = _source ?? throw new BuilderMissingPropertyException("source");

        var root = _root ?? throw new BuilderMissingPropertyException("root");

        return new EmbeddedResourceTree(source, root);
    }

    #endregion

}
