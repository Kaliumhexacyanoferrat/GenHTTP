using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

using Microsoft.DocAsCode.MarkdownLite;

namespace GenHTTP.Modules.Markdown
{

    public class MarkdownRenderer<T> : IRenderer<T> where T : class, IBaseModel
    {
        private static readonly IMarkdownEngine _Engine = new GfmEngineBuilder(new Options()).CreateEngine(new HtmlRenderer());

        private string? _Markdown;

        #region Get-/Setters

        public CachedResource File { get; }

        #endregion

        #region Initialization

        public MarkdownRenderer(IResourceProvider fileProvider)
        {
            File = new CachedResource(fileProvider);
        }

        #endregion

        #region Functionality

        public string Render(T? model) => _Engine.Markup(LoadFile());

        private string LoadFile()
        {
            if (File.Changed)
            {
                _Markdown = File.GetResourceAsString();
            }

            return _Markdown!;
        }

        #endregion

    }
    
}
