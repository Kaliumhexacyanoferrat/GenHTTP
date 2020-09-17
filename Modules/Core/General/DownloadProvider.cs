using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.General
{

    public class DownloadProvider : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResourceProvider ResourceProvider { get; }

        private FlexibleContentType ContentType { get; }

        #endregion

        #region Initialization

        public DownloadProvider(IHandler parent, IResourceProvider resourceProvider, FlexibleContentType contentType)
        {
            Parent = parent;

            ResourceProvider = resourceProvider;
            ContentType = contentType;
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            if (!request.HasType(RequestMethod.GET, RequestMethod.HEAD))
            {
                return this.MethodNotAllowed(request).Build();
            }

            return request.Respond()
                          .Content(ResourceProvider.GetResource())
                          .Type(ContentType)
                          .Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, "Download", ContentType);

        #endregion

    }

}
