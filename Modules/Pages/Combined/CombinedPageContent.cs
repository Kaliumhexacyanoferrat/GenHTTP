using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Pages.Combined
{

    public class CombinedPageContent : IResponseContent
    {

        #region Get-/Setters

        public ulong? Length => null;

        private List<ContentFragment> Fragments { get; }

        private ContentInfo PageInfo { get; }

        private IHandler Handler { get; }

        private IRequest Request { get; }

        #endregion

        #region Initialization

        public CombinedPageContent(List<ContentFragment> fragments, ContentInfo pageInfo, IHandler handler, IRequest request)
        {
            Fragments = fragments;
            PageInfo = pageInfo;

            Handler = handler;
            Request = request;
        }

        #endregion

        #region Functionality

        public async ValueTask<ulong?> CalculateChecksumAsync()
        {
            unchecked
            {
                ulong hash = 17;

                foreach (var fragment in Fragments)
                {
                    hash = hash * 23 + await fragment.Renderer.CalculateChecksumAsync().ConfigureAwait(false);
                    hash = hash * 23 + await fragment.Model.CalculateChecksumAsync();
                }

                var pageRenderer = Handler.GetPageRenderer(Request);

                hash = hash * 23 + await pageRenderer.CalculateChecksumAsync();

                hash = hash * 23 + (uint)(PageInfo.Description?.GetHashCode() ?? 0);
                hash = hash * 23 + (uint)(PageInfo.Title?.GetHashCode() ?? 0);

                return hash;
            }
        }

        public async ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            var builder = new StringBuilder();

            foreach (var fragment in Fragments)
            {
                builder.AppendLine(await fragment.Renderer.RenderAsync(fragment.Model));
            }

            await Handler.WritePageAsync(Request, PageInfo, builder.ToString(), target);
        }

        #endregion

    }
}
