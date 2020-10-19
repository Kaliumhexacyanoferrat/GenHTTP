using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websites.Bundles
{

    public class BundleProvider : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public FlexibleContentType ContentType { get; }

        private IEnumerable<IResource> Items { get; }

        #endregion

        #region Initialization

        public BundleProvider(IHandler parent, IEnumerable<IResource> items, FlexibleContentType contentType)
        {
            Parent = parent;

            ContentType = contentType;
            Items = items;
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            return request.Respond()
                          .Content(new BundleContent(Items))
                          .Type(ContentType)
                          .Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => Enumerable.Empty<ContentElement>();

        #endregion

    }

}
