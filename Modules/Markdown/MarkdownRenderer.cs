using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Modules.Basics;
using Microsoft.DocAsCode.MarkdownLite;

namespace GenHTTP.Modules.Markdown
{
    public class MarkdownRenderer<T> : IRenderer<T> where T : class, IBaseModel
    {
        private readonly IResourceProvider _FileProvider;
        private static readonly IMarkdownEngine _Engine = new GfmEngineBuilder(new Options()).CreateEngine(new HtmlRenderer());

        public MarkdownRenderer(IResourceProvider fileProvider)
        {
            _FileProvider = fileProvider;
        }

        public string Render(T? model)
        {
            var markdown = _FileProvider.GetResourceAsString();
            return _Engine.Markup(markdown);
        }
    }
}