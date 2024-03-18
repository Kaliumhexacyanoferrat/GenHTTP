using System;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using Microsoft.IO;

namespace GenHTTP.Modules.Pages.Rendering
{

    public sealed class RenderedContent<T> : IResponseContent, IDisposable where T : class, IModel
    {
        private static readonly RecyclableMemoryStreamManager STREAM_MANAGER = new RecyclableMemoryStreamManager();

        private RecyclableMemoryStream? _Buffer = null;

        private bool _Disposed;

        #region Get-/Setters

        public ulong? Length => null;

        private IRenderer<T> Renderer { get; }

        private T Model { get; }

        private ContentInfo PageInfo { get; }

        private PageAdditions? Additions { get; }

        #endregion

        #region Initialization

        public RenderedContent(IRenderer<T> renderer, T model, ContentInfo pageInfo, PageAdditions? additions)
        {
            Renderer = renderer;
            Model = model;

            PageInfo = pageInfo;
            Additions = additions;
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

        /// <summary>
        /// Renders the page into a temporary buffer which is used later on to
        /// actually write to the response stream.
        /// </summary>
        /// <remarks>
        /// Required to be able to handle rendering errors, because otherwise
        /// the response headers would have been written already.
        /// </remarks>
        public async ValueTask FillBufferAsync()
        {
            if (_Buffer != null)
            {
                _Buffer.Dispose();
                _Buffer = null;
            }

            var buffer = new RecyclableMemoryStream(STREAM_MANAGER);

            await WriteContent(buffer);

            buffer.Seek(0, SeekOrigin.Begin);

            _Buffer = buffer;
        }

        public async ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            if (_Buffer != null)
            {
                await _Buffer.CopyToAsync(target, (int)bufferSize);
            }
            else
            {
                await WriteContent(target);
            }
        }

        private async ValueTask WriteContent(Stream target)
        {
            var request = Model.Request;

            var handler = Model.Handler;

            var pageContent = await Renderer.RenderAsync(Model).ConfigureAwait(false);

            await handler.WritePageAsync(request, PageInfo, Additions, pageContent, target);
        }

        #endregion

        #region Disposal

        private void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    _Buffer?.Dispose();
                }

                _Buffer = null;
                _Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
