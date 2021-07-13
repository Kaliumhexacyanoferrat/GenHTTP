using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Pages.Rendering
{

    public sealed class RenderedContent<T> : IResponseContent where T : class, IModel
    {

        #region Get-/Setters

        public ulong? Length => null;

        private IRenderer<T> Renderer { get; }

        private T Model { get; }

        private ContentInfo PageInfo { get; }

        #endregion

        #region Initialization

        public RenderedContent(IRenderer<T> renderer, T model, ContentInfo pageInfo)
        { 
            Renderer = renderer;
            Model = model;

            PageInfo = pageInfo;
        }

        #endregion

        #region Functionality

        public async ValueTask<ulong?> CalculateChecksumAsync()
        {
            unchecked
            {
                ulong hash = 17;

                hash = hash * 23 + await Model.CalculateChecksumAsync().ConfigureAwait(false);

                hash = hash * 23 + (uint)(PageInfo.Description?.GetHashCode() ?? 0);
                hash = hash * 23 + (uint)(PageInfo.Title?.GetHashCode() ?? 0);

                var pageRenderer = Model.Handler.GetPageRenderer(Model.Request);

                hash = hash * 23 + await Renderer.CalculateChecksumAsync();
                hash = hash * 23 + await pageRenderer.CalculateChecksumAsync();

                return hash;
            }
        }

        public async ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            var request = Model.Request;

            var handler = Model.Handler;

            var pageContent = await Renderer.RenderAsync(Model).ConfigureAwait(false);

            await handler.WritePageAsync(request, PageInfo, pageContent, target);
        }

        #endregion

    }

}
