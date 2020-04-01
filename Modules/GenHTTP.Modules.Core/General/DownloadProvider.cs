using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;
using System.Collections.Generic;

namespace GenHTTP.Modules.Core.General
{

    public class DownloadProvider : ContentProviderBase
    {

        #region Get-/Setters

        public IResourceProvider ResourceProvider { get; }

        public override string? Title => null;

        public override FlexibleContentType? ContentType { get; }

        protected override HashSet<FlexibleRequestMethod>? SupportedMethods => _GET;

        #endregion

        #region Initialization

        public DownloadProvider(IResourceProvider resourceProvider, FlexibleContentType contentType, ResponseModification? mod) : base(mod)
        {
            ResourceProvider = resourceProvider;
            ContentType = contentType;
        }

        #endregion

        #region Functionality

        protected override IResponseBuilder HandleInternal(IRequest request)
        {
            return request.Respond()
                          .Content(ResourceProvider.GetResource())
                          .Type(ContentType!.Value);
        }

        #endregion

    }

}
