using System.Collections.Concurrent;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.IO.Providers;

public sealed class ResourceHandler : IHandler
{
    private readonly ConcurrentDictionary<IResource, ResourceContent> _contentCache = new();
    
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
        var (_, resource) = await Tree.Find(request.Header.Target);

        if (resource is not null)
        {
            var content = _contentCache.GetOrAdd(resource, static r => new ResourceContent(r));

            return request.Respond()
                          .Content(content)
                          .Build();
        }

        return null;
    }

    #endregion

}
