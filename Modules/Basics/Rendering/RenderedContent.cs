using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Basics.Rendering
{

    public class RenderedContent<T> : IResponseContent where T : PageModel
    {
        private static readonly Encoding _ENCODING = Encoding.UTF8;

        #region Get-/Setters

        public ulong? Length => throw new NotImplementedException();

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

        public ValueTask<ulong?> CalculateChecksumAsync()
        {
            throw new NotImplementedException();
        }

        public async ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            var request = Model.Request;

            var handler = Model.Handler;

            var pageContent = await Renderer.RenderAsync(Model).ConfigureAwait(false);

            var templateModel = new TemplateModel(request, handler, PageInfo, pageContent);

            var templateRenderer = handler.FindParent<IPageRenderer>(request.Server.Handler) ?? throw new InvalidOperationException("There is no page renderer available in the routing tree");

            var content = await templateRenderer.RenderAsync(templateModel);

            var buffer = _ENCODING.GetBytes(content);

            await target.WriteAsync(buffer.AsMemory(0, buffer.Length));
        }

        #endregion

    }

}
