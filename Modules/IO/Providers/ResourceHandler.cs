using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.IO.Providers;

public sealed class ResourceHandler : IHandler
{

    #region Get-/Setters

    private IResourceTree Tree { get; }

    #endregion

    #region Initialization

    public ResourceHandler(IResourceTree tree)
    {
        Tree = tree;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var (_, resource) = await Tree.FindAsync(request.Header.Target);

        if (resource is not null)
        {
            return request.Respond()
                          .Content(new ResourceContent(resource))
                          .Build();
        }

        return null;
    }

    #endregion

}
