using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.Files.Handlers;

public class TreeAssetsHandler : IHandler
{
    private readonly IResourceTree _tree;

    public TreeAssetsHandler(IResourceTree tree)
    {
        _tree = tree;
    }

    public ValueTask PrepareAsync() => default;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var (_, resource) = await _tree.FindAsync(request.Header.Target);

        if (resource is not null)
        {
            return request.Respond()
                          .Content(new ResourceContent(resource))
                          .Build();
        }

        return null;
    }

}
