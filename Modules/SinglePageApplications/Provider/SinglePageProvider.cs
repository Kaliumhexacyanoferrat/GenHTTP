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

        #region Get-/Setters

        public IHandler Parent { get; }

        private IResourceTree Tree { get; }

        private IHandler? Index { get; }

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

            foreach (var index in INDEX_FILES)
            {
                if (tree.TryGetResource(index, out var indexFile))
                {
                    Index = Content.From(indexFile)
                                   .Build(this);

                    break;
                }
            }
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            if (request.Target.Ended)
            {
                if (Index != null)
                {
                    return await Index.HandleAsync(request);
                }
            }
            else
            {
                var result = await Resources.HandleAsync(request);

                if ((result == null) && (Index != null) && ServerSideRouting)
                {
                    return await Index.HandleAsync(request);
                }

                return result;
            }

            return null;
        }

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IEnumerable<ContentElement> GetContent(IRequest request) => Tree.GetContent(request, this);

        #endregion

    }

}
