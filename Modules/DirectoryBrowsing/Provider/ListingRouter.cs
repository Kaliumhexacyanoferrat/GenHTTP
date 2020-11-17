using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider
{

    public class ListingRouter : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private IResourceTree Tree { get; }

        #endregion

        #region Initialization

        public ListingRouter(IHandler parent, IResourceTree tree)
        {
            Parent = parent;
            Tree = tree;
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var (node, resource) = Tree.Find(request.Target);

            if (resource is not null)
            {
                return await Content.From(resource)
                                    .Build(this)
                                    .HandleAsync(request)
                                    .ConfigureAwait(false);
            }
            else if (node is not null)
            {
                return await new ListingProvider(this, node).HandleAsync(request)
                                                            .ConfigureAwait(false);
            }

            return null;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            return Tree.GetContent(request, this, (path, children) =>
            {
                var info = new ContentInfo($"Index of {path}", null);

                return new ContentElement(path, info, ContentType.TextHtml, children);
            });
        }

        #endregion

    }

}
