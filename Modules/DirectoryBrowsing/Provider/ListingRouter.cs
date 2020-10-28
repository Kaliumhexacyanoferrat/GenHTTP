using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
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

        public IResponse? Handle(IRequest request)
        {
            var (node, resource) = Tree.Find(request.Target);

            if (resource != null)
            {
                return Content.From(resource)
                              .Build(this)
                              .Handle(request);
            }
            else if (node != null)
            {
                return new ListingProvider(this, node).Handle(request);
            }

            return null;
        }

        // todo: somehow add index pages

        public IEnumerable<ContentElement> GetContent(IRequest request) => Tree.GetContent(request, this);

        #endregion

    }

}
