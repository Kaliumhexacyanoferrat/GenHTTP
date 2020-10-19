using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

using Markdig;

namespace GenHTTP.Modules.Markdown
{

    public class MarkdownRenderer<T> : IRenderer<T> where T : class, IBaseModel
    {
        private static readonly MarkdownPipeline PIPELINE = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

        private string? _Markdown;

        #region Get-/Setters

        public CachedResource File { get; }

        #endregion

        #region Initialization

        public MarkdownRenderer(IResource fileProvider)
        {
            File = new CachedResource(fileProvider);
        }

        #endregion

        #region Functionality

        public string Render(T? model) => Markdig.Markdown.ToHtml(LoadFile(), PIPELINE);

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
