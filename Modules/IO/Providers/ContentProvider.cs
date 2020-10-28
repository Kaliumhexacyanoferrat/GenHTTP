using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO.Providers
{

    public class ContentProvider : IHandler
    {
        private ContentInfo? _Info;

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResource Resource { get; }

        private FlexibleContentType ContentType => Resource.ContentType ?? new FlexibleContentType(Api.Protocol.ContentType.ApplicationForceDownload);

        private ContentInfo Info
        {
            get
            {
                return _Info ??= ContentInfo.Create()
                                            .Title(Resource.Name ?? "Download")
                                            .Build();
            }
        }

        #endregion

        #region Initialization

        public ContentProvider(IHandler parent, IResource resourceProvider)
        {
            Parent = parent;
            Resource = resourceProvider;
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            // todo: in tatsächlichen download-provider schieben?

            /*if (!request.HasType(RequestMethod.GET, RequestMethod.HEAD))
            {
                return this.MethodNotAllowed(request).Build();
            }*/

            // todo: attachment-name? optional für richtige downloads? sowas wie "DownloadFileName?"

            return request.Respond()
                          .Content(Resource)
                          .Type(ContentType)
                          .Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, Info, ContentType);

        #endregion

    }

}
