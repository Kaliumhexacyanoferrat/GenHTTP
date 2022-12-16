using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO.Tracking;
using GenHTTP.Modules.IO.Streaming;

using Markdig;

namespace GenHTTP.Modules.Markdown
{

    public sealed class MarkdownRenderer<T> : IRenderer<T> where T : class, IModel
    {
        private static readonly MarkdownPipeline PIPELINE = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

        private string? _Markdown;

        #region Get-/Setters

        public ChangeTrackingResource File { get; }

        #endregion

        #region Initialization

        public MarkdownRenderer(IResource fileProvider)
        {
            File = new(fileProvider);
        }

        #endregion

        #region Functionality

        public ValueTask<ulong> CalculateChecksumAsync() => File.CalculateChecksumAsync();

        public async ValueTask<string> RenderAsync(T? model) => Markdig.Markdown.ToHtml(await GetContent(), PIPELINE);

        public ValueTask RenderAsync(T model, Stream target) => this.RenderToStream(model, target);

        public async ValueTask PrepareAsync()
        {
            if (_Markdown is null)
            {
                await LoadFile();
            }
        }

        private async ValueTask<string> GetContent()
        {
            if (await File.HasChanged())
            {
                await LoadFile();
            }

            return _Markdown!;
        }

        private async ValueTask LoadFile()
        {
            _Markdown = await File.GetResourceAsStringAsync();
        }

        #endregion

    }
    
}
