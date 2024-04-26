using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Minification.Plugins
{

    public abstract class TextBasedMinificationResult : IResponseContent
    {

        #region Get-/Setters

        public ulong? Length => null;

        protected IResponseContent Original { get; }

        protected MinificationErrors ErrorHandling { get; }

        #endregion

        #region Initialization

        public TextBasedMinificationResult(IResponseContent original, MinificationErrors errorHandling)
        {
            Original = original;
            ErrorHandling = errorHandling;
        }

        #endregion

        #region Functionality

        public ValueTask<ulong?> CalculateChecksumAsync() => Original.CalculateChecksumAsync();

        public async ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            using var inputStream = new MemoryStream();

            await Original.WriteAsync(inputStream, bufferSize);

            inputStream.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(inputStream);

            var content = await reader.ReadToEndAsync();

            string transformed;

            try
            {
                transformed = Transform(content, ErrorHandling == MinificationErrors.Ignore);
            }
            catch
            {
                if (ErrorHandling == MinificationErrors.ServeOriginal)
                {
                    transformed = content;
                }
                else
                {
                    throw;
                }
            }

            using var writer = new StreamWriter(target, bufferSize: (int)bufferSize, leaveOpen: true);

            await writer.WriteAsync(transformed);
        }

        protected abstract string Transform(string input, bool ignoreErrors);

        #endregion

    }

}
