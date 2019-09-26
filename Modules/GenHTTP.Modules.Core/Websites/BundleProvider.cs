using System.Collections.Generic;
using System.IO;

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
            // ToDo: Custom stream implementation for less copying
            var newLine = new byte[] { (byte)'\n' };

            var content = new MemoryStream();

            foreach (var item in Items)
            {
                using var source = item.GetResource();
                source.CopyTo(content);

                content.Write(newLine, 0, 1);
            }

            content.Seek(0, SeekOrigin.Begin);

            return request.Respond()
                          .Content(content, ContentType.RawType);
        }

        #endregion

    }

}
