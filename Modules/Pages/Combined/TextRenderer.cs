using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.Pages.Combined
{

    public class TextRenderer : IRenderer<IModel>
    {

        #region Get-/Setters

        public string Content { get; }

        private byte[] ByteContent { get; }

        #endregion

        #region Initialization

        public TextRenderer(string content)
        {
            Content = content;

            ByteContent = Encoding.UTF8.GetBytes(content);
        }

        #endregion

        #region Functionality

        public ValueTask<ulong> CalculateChecksumAsync() => new((ulong)Content.GetHashCode());

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public ValueTask<string> RenderAsync(IModel model) => new(Content);

        public ValueTask RenderAsync(IModel model, Stream target) => target.WriteAsync(ByteContent.AsMemory());

        #endregion

    }

}
