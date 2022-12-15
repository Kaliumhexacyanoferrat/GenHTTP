using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider
{

    public sealed class ListingProvider : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResourceContainer Container { get; }

        #endregion

        #region Initialization

        public ListingProvider(IHandler parent, IResourceContainer container)
        {
            Parent = parent;
            Container = container;
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var model = new ListingModel(request, this, Container, !request.Target.Ended);

            var renderer = new ListingRenderer();

            var templateModel = new TemplateModel(request, this, GetPageInfo(request), await renderer.RenderAsync(model).ConfigureAwait(false));

            return (await this.GetPageAsync(request, templateModel).ConfigureAwait(false)).Build();
        }

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => AsyncEnumerable.Empty<ContentElement>();

        private static ContentInfo GetPageInfo(IRequest request)
        {
            return ContentInfo.Create()
                              .Title($"Index of {request.Target.Path}")
                              .Build();
        }

        #endregion

    }

}
