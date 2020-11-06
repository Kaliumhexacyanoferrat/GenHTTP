using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO.Tracking;

using Markdig;

using PooledAwait;

namespace GenHTTP.Modules.Markdown
{

    public class MarkdownRenderer<T> : IRenderer<T> where T : class, IBaseModel
    {
        private static readonly MarkdownPipeline PIPELINE = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

        private string? _Markdown;

        #region Get-/Setters

        public ChangeTrackingResource File { get; }

        #endregion

        #region Initialization

        public MarkdownRenderer(IResource fileProvider)
        {
            File = new ChangeTrackingResource(fileProvider);
        }

        #endregion

        #region Functionality

        public async ValueTask<string> RenderAsync(T? model) => Markdig.Markdown.ToHtml(await LoadFile(), PIPELINE);

        private async PooledValueTask<string> LoadFile()
        {
            if (await File.HasChanged())
            {
                _Markdown = await File.GetResourceAsStringAsync();
            }

            return _Markdown!;
        }

        #endregion

    }
    
}
