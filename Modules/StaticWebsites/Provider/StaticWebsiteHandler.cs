using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.StaticWebsites.Provider;

public sealed class StaticWebsiteHandler : IHandler
{
    private static readonly string[] IndexFiles = ["index.html", "index.htm"];

    #region Initialization

    public StaticWebsiteHandler(IHandler parent, IResourceTree tree)
    {
        Parent = parent;
        Tree = tree;

        Resources = IO.Resources.From(tree)
                      .Build(this);
    }

    #endregion

    #region Get-/Setters

    public IHandler Parent { get; }

    private IResourceTree Tree { get; }

    private IHandler Resources { get; }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (request.Target.Path.TrailingSlash)
        {
            var (node, _) = await Tree.Find(request.Target);

            if (node != null)
            {
                foreach (var indexFile in IndexFiles)
                {
                    IResource? file;

                    if ((file = await node.TryGetResourceAsync(indexFile)) != null)
                    {
                        return await Content.From(file)
                                            .Build(this)
                                            .HandleAsync(request);
                    }
                }
            }

            return null;
        }
        return await Resources.HandleAsync(request);
    }

    public async ValueTask PrepareAsync()
    {
        await Resources.PrepareAsync();
    }

    #endregion

}
