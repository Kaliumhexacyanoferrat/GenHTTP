using System;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

using PooledAwait;

namespace GenHTTP.Modules.IO.Streaming
{

    public class StreamContent : IResponseContent, IDisposable
    {
        private readonly Func<ValueTask<ulong?>> _ChecksumProvider;

        #region Get-/Setters

        private Stream Content { get; }

        public ulong? Length
        {
            get
            {
                if (Content.CanSeek)
                {
                    return (ulong)Content.Length;
                }

                return null;
            }
        }

        #endregion

        #region Initialization

        public StreamContent(Stream content, Func<ValueTask<ulong?>> checksumProvider)
        {
            Content = content;
            _ChecksumProvider = checksumProvider;
        }

        #endregion

        #region Functionality

        public ValueTask<ulong?> CalculateChecksumAsync() => _ChecksumProvider();

        public ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            return DoWrite(this, target, bufferSize);

            static PooledValueTask DoWrite(StreamContent self, Stream target, uint bufferSize)
            {
                return self.Content.CopyPooledAsync(target, bufferSize);
            }
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
                    Content.Dispose();
                }

                disposedValue = true;
            }
        }

        ~StreamContent()
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
