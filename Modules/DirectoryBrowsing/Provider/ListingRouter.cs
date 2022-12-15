using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider
{

    public sealed class ListingRouter : IHandler
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
            var (node, resource) = await Tree.Find(request.Target).ConfigureAwait(false);

            if (resource is not null)
            {
                return await Content.From(resource)
                                    .Build(this)
                                    .HandleAsync(request);
            }
            else if (node is not null)
            {
                return await new ListingProvider(this, node).HandleAsync(request);
            }

            return null;
        }

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request)
        {
            return Tree.GetContent(request, this, (_, path, children) =>
            {
                var info = new ContentInfo($"Index of {path}", null);

                return new(new ContentElement(path, info, ContentType.TextHtml, children.ToEnumerable()));
            });
        }
        
        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        #endregion

    }

}
