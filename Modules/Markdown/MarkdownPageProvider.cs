using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Markdown
{

    public sealed class MarkdownPageProvider<T> : IHandler where T : class, IModel
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public MarkdownRenderer<T> Renderer { get; }

        public ContentInfo PageInfo { get; }

        public PageAdditions? Additions { get; }

        public ResponseModifications? Modifications { get; }

        #endregion

        #region Initialization

        public MarkdownPageProvider(IHandler parent, IResource fileProvider, ContentInfo pageInfo, PageAdditions? additions, ResponseModifications? modifications)
        {
            Parent = parent;

            PageInfo = pageInfo;
            Additions = additions;
            Modifications = modifications;

            Renderer = new(fileProvider);
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => Renderer.PrepareAsync();

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => this.GetContent(request, PageInfo, ContentType.TextHtml);

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var content = await Renderer.RenderAsync(null)
                                        ;

            var templateModel = new TemplateModel(request, this, PageInfo, Additions, content);

            var response = await this.GetPageAsync(request, templateModel);

            if (Modifications != null)
            {
                Modifications.Apply(response);
            }

            return response.Build();
        }

        #endregion

    }

}