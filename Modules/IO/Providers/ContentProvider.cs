using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.IO.Providers
{

    public sealed class ContentProvider : IHandler
    {
        private ContentInfo? _Info;

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResource Resource { get; }

        private IResponseContent Content { get; }

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

            Content = new ResourceContent(Resource);
            ContentType = Resource.ContentType ?? FlexibleContentType.Get(Resource.Name?.GuessContentType() ?? Api.Protocol.ContentType.ApplicationForceDownload);
        }

        #endregion

        #region Functionality

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            return request.Respond()
                          .Content(Content)
                          .Type(ContentType)
                          .BuildTask();
        }

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => this.GetContent(request, Info, ContentType);

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        #endregion

    }

}
