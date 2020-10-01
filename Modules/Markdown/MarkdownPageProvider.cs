using System.Collections.Generic;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Markdown
{
    public class MarkdownPageProvider<T> : IHandler where T : PageModel
    {
        public IHandler Parent { get; }

        public string? Title { get; }

        public MarkdownRenderer<T> Renderer { get; }

        public MarkdownPageProvider(IHandler parent, IResourceProvider fileProvider, string? title)
        {
            Title = title;
            Parent = parent;

            Renderer = new MarkdownRenderer<T>(fileProvider);
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, Title ?? "Untitled Page", ContentType.TextHtml);

        public IResponse Handle(IRequest request)
        {
            var content = Renderer.Render(null);

            var templateModel = new TemplateModel(request, this, Title ?? "Untitled Page", content);

            return this
                .Page(templateModel)
                .Build();
        }
    }
}