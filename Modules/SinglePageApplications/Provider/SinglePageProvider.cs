using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.SinglePageApplications.Provider;

public sealed class SinglePageProvider : IHandler
{
    private static readonly HashSet<string> IndexFiles = new(StringComparer.InvariantCultureIgnoreCase)
    {
        "index.html",
        "index.htm"
    };

    private IHandler? _index;

    #region Get-/Setters

    private IResourceTree Tree { get; }

    private IHandler Resources { get; }

    private bool ServerSideRouting { get; }

    #endregion

    #region Initialization

    public SinglePageProvider(IResourceTree tree, bool serverSideRouting)
    {
        Tree = tree;
        ServerSideRouting = serverSideRouting;

        Resources = IO.Resources.From(tree)
                      .Build();
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (request.Target.Ended)
        {
            var index = await GetIndex();

            if (index != null)
            {
                return await index.HandleAsync(request);
            }
        }
        else
        {
            var result = await Resources.HandleAsync(request);

            if (result == null)
            {
                var index = await GetIndex();

                if (index != null && ServerSideRouting)
                {
                    return await index.HandleAsync(request);
                }
            }

            return result;
        }

        return null;
    }

    private async ValueTask<IHandler?> GetIndex()
    {
        if (_index == null)
        {
            foreach (var index in IndexFiles)
            {
                IResource? indexFile;

                if ((indexFile = await Tree.TryGetResourceAsync(index)) != null)
                {
                    _index = Content.From(indexFile)
                                    .Build();

                    break;
                }
            }
        }

        return _index;
    }

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    #endregion

}
