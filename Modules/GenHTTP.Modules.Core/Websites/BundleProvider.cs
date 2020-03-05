using System.Collections.Generic;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Websites
{

    public class BundleProvider : IContentProvider
    {

        #region Get-/Setters

        public string? Title => null;

        public FlexibleContentType ContentType { get; }

        private IEnumerable<IResourceProvider> Items { get; }

        #endregion

        #region Initialization

        public BundleProvider(IEnumerable<IResourceProvider> items, FlexibleContentType contentType)
        {
            ContentType = contentType;
            Items = items;
        }

        #endregion

        #region Functionality

        public IResponseBuilder Handle(IRequest request)
        {
            return request.Respond()
                          .Content(new BundleContent(Items))
                          .Type(ContentType);
        }

        #endregion

    }

}
