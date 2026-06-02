using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.Files.Handlers;

public sealed class TreeAssetsHandler : AbstractAssetsHandler
{
    private readonly IResourceTree _tree;

    public TreeAssetsHandler(IResourceTree tree, List<ICompressionAlgorithm> algorithms, char separator) : base(algorithms, separator)
    {
        _tree = tree;
    }

    protected override async ValueTask<IResponseContent?> Resolve(IRequestTarget target)
    {
        var (_, resource) = await _tree.FindAsync(target);

        if (resource is not null)
        {
            return new ResourceContent(resource);
        }

        return null;
    }

}
