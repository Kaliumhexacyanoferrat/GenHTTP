using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider;

public sealed class ListingRouter : IHandler
{

    #region Get-/Setters

    private IResourceTree Tree { get; }

    #endregion

    #region Initialization

    public ListingRouter(IResourceTree tree)
    {
        Tree = tree;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var (node, resource) = await Tree.Find(request.Target);

        if (resource is not null)
        {
            return await Content.From(resource)
                                .Build()
                                .HandleAsync(request);
        }
        if (node is not null)
        {
            return await new ListingProvider(node).HandleAsync(request);
        }

        return null;
    }

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    #endregion

}
