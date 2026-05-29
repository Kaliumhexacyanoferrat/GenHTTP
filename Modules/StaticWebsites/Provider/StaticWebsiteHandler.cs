using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.StaticWebsites.Provider;

public sealed class StaticWebsiteHandler : IHandler
{
    private static readonly PathSegment[] IndexFiles = [ new("index.html"), new("index.htm") ];

    #region Get-/Setters

    private IResourceTree Tree { get; }

    private IHandler Resources { get; }

    #endregion

    #region Initialization

    public StaticWebsiteHandler(IResourceTree tree)
    {
        Tree = tree;

        Resources = IO.Resources.From(tree)
                      .Build();
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var target = request.Header.Target;

        if (target.HasTrailingSlash)
        {
            var (node, _) = await Tree.FindAsync(target);

            if (node != null)
            {
                foreach (var indexFile in IndexFiles)
                {
                    var file = await node.TryGetResourceAsync(indexFile);

                    if (file != null)
                    {
                        return request.Respond()
                                      .Status(ResponseStatus.Ok)
                                      .Content(new ResourceContent(file))
                                      .Build();
                    }
                }
            }

            return null;
        }

        return await Resources.HandleAsync(request);
    }

    public ValueTask PrepareAsync() => Resources.PrepareAsync();

    #endregion

}
