using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Resources
{

    public class ResourceHandler : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        protected IResourceTree Tree { get; }

        #endregion

        #region Initialization

        public ResourceHandler(IHandler parent, IResourceTree tree)
        {
            Parent = parent;
            Tree = tree;
        }

        #endregion

        #region Functionality

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            return Tree.GetContent(request);
        }

        public IResponse? Handle(IRequest request)
        {
            var found = Tree.Find(request.Target);

            if (found != null)
            {
                // ToDo: Improve performance, don't build

                return Download.From(found)
                               .Build(this)
                               .Handle(request);
            }

            return null;
        }

        #endregion

    }

}
