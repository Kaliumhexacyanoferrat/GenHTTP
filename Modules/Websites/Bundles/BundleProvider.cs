using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websites.Bundles
{

    public sealed class BundleProvider : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public FlexibleContentType ContentType { get; }

        private BundleContent Bundle { get; }

        #endregion

        #region Initialization

        public BundleProvider(IHandler parent, IEnumerable<IResource> items, FlexibleContentType contentType)
        {
            Parent = parent;

            ContentType = contentType;
            Bundle = new BundleContent(items);
        }

        #endregion

        #region Functionality

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var response = request.Respond()
                                  .Content(Bundle)
                                  .Type(ContentType)
                                  .Build();

            return new ValueTask<IResponse?>(response);
        }

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => AsyncEnumerable.Empty<ContentElement>();

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        #endregion

    }

}
