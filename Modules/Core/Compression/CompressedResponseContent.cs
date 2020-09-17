using System;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Compression
{

    public class CompressedResponseContent : IResponseContent, IDisposable
    {

        #region Get-/Setters

        public ulong? Length => null;

        private IResponseContent OriginalContent { get; }

        private Func<Stream, Stream> Generator { get; }

        #endregion

        #region Initialization

        public CompressedResponseContent(IResponseContent originalContent, Func<Stream, Stream> generator)
        {
            OriginalContent = originalContent;
            Generator = generator;
        }

        #endregion

        #region Functionality

        public async Task Write(Stream target, uint bufferSize)
        {
            using var compressed = Generator(target);

            await OriginalContent.Write(compressed, bufferSize);
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (OriginalContent is IDisposable disposableContent)
                    {
                        disposableContent.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        ~CompressedResponseContent()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
