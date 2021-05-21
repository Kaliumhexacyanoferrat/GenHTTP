using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO.Tracking;

using Markdig;

using PooledAwait;

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

        public async ValueTask PrepareAsync()
        {
            if (_Markdown is null)
            {
                await LoadFile();
            }
        }

        private async PooledValueTask<string> GetContent()
        {
            if (await File.HasChanged())
            {
                await LoadFile();
            }

            return _Markdown!;
        }

        private async PooledValueTask LoadFile()
        {
            _Markdown = await File.GetResourceAsStringAsync();
        }

        #endregion

    }
    
}
