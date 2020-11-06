using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Markdown
{

    public class MarkdownPageProvider<T> : IHandler where T : PageModel
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public MarkdownRenderer<T> Renderer { get; }

        public ContentInfo PageInfo { get; }

        #endregion

        #region Initialization

        public MarkdownPageProvider(IHandler parent, IResource fileProvider, ContentInfo pageInfo)
        {
            Parent = parent;

            PageInfo = pageInfo;
            Renderer = new MarkdownRenderer<T>(fileProvider);
        }

        #endregion

        #region Functionality

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, PageInfo, ContentType.TextHtml);

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var content = await Renderer.RenderAsync(null)
                                        .ConfigureAwait(false);

            var templateModel = new TemplateModel(request, this, PageInfo, content);

            return (await this.GetPageAsync(templateModel).ConfigureAwait(false)).Build();
        }

        #endregion

    }

}