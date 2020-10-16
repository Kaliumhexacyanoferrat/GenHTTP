using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO.Providers
{

    public class DownloadProvider : IHandler
    {
        private ContentInfo? _Info;

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResourceProvider ResourceProvider { get; }

        private FlexibleContentType ContentType { get; }

        private ContentInfo Info
        {
            get
            {
                return _Info ??= ContentInfo.Create()
                                            .Title("Download")
                                            .Build();
            }
        }

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
                          .Content(ResourceProvider)
                          .Type(ContentType)
                          .Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, Info, ContentType);

        #endregion

    }

}
