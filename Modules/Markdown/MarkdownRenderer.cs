using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Basics;

using Microsoft.DocAsCode.MarkdownLite;

namespace GenHTTP.Modules.Markdown
{

    public class MarkdownRenderer<T> : IRenderer<T> where T : class, IBaseModel
    {
        private static readonly IMarkdownEngine _Engine = new GfmEngineBuilder(new Options()).CreateEngine(new HtmlRenderer());
        
        private readonly IResourceProvider _FileProvider;

        public MarkdownRenderer(IResourceProvider fileProvider)
        {
            _FileProvider = fileProvider;
        }

        public string Render(T? model) => _Engine.Markup(_FileProvider.GetResourceAsString());
        
    }
    
}
