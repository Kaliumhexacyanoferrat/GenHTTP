using System.Collections.Generic;
using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO.Providers
{

    public sealed class ContentProvider : IHandler
    {
        private ContentInfo? _Info;

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResource Resource { get; }

        private FlexibleContentType ContentType { get; }

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
            ContentType = Resource.ContentType ?? FlexibleContentType.Get(Resource.Name?.GuessContentType() ?? Api.Protocol.ContentType.ApplicationForceDownload);
        }

        #endregion

        #region Functionality

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            return request.Respond()
                          .Content(Resource)
                          .Type(ContentType)
                          .BuildTask();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, Info, ContentType);

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        #endregion

    }

}
