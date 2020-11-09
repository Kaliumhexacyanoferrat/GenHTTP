using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO.Providers
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
            return Tree.GetContent(request, this);
        }

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var (_, resource) = Tree.Find(request.Target);

            if (resource != null)
            {
                var type = resource.ContentType ?? new FlexibleContentType(resource.Name?.GuessContentType() ?? ContentType.ApplicationForceDownload);

                return request.Respond()
                              .Content(resource)
                              .Type(type)
                              .BuildTask();
            }

            return new ValueTask<IResponse?>();
        }

        #endregion

    }

}
