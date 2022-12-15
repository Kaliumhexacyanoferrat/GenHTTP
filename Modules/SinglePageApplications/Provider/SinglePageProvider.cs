using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.SinglePageApplications.Provider
{

    public sealed class SinglePageProvider : IHandler
    {
        private static readonly HashSet<string> INDEX_FILES = new(StringComparer.InvariantCultureIgnoreCase)
        {
            "index.html", "index.htm"
        };

        private IHandler? _Index;

        #region Get-/Setters

        public IHandler Parent { get; }

        private IResourceTree Tree { get; }

        private IHandler Resources { get; }

        private bool ServerSideRouting { get; }

        #endregion

        #region Initialization

        public SinglePageProvider(IHandler parent, IResourceTree tree, bool serverSideRouting)
        {
            Parent = parent;

            Tree = tree;
            ServerSideRouting = serverSideRouting;

            Resources = IO.Resources.From(tree)
                                    .Build(this);
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            if (request.Target.Ended)
            {
                var index = await GetIndex().ConfigureAwait(false);

                if (index != null)
                {
                    return await index.HandleAsync(request);
                }
            }
            else
            {
                var result = await Resources.HandleAsync(request).ConfigureAwait(false);

                if (result == null)
                {
                    var index = await GetIndex();

                    if ((index != null) && ServerSideRouting)
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
            if (_Index == null)
            {
                foreach (var index in INDEX_FILES)
                {
                    IResource? indexFile;

                    if ((indexFile = await Tree.TryGetResourceAsync(index)) != null)
                    {
                        _Index = Content.From(indexFile)
                                        .Build(this);

                        break;
                    }
                }
            }

            return _Index;
        }

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Tree.GetContent(request, this);

        #endregion

    }

}
