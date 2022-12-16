using System;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Streaming
{

    public class StreamContent : IResponseContent, IDisposable
    {
        private readonly Func<ValueTask<ulong?>> _ChecksumProvider;

        private readonly ulong? _KnownLengh;

        #region Get-/Setters

        private Stream Content { get; }

        public ulong? Length
        {
            get
            {
                if (_KnownLengh != null)
                {
                    return _KnownLengh;
                }

                if (Content.CanSeek)
                {
                    return (ulong)Content.Length;
                }

                return null;
            }
        }

        #endregion

        #region Initialization

        public StreamContent(Stream content, ulong? knownLength, Func<ValueTask<ulong?>> checksumProvider)
        {
            Content = content;

            _KnownLengh = knownLength;
            _ChecksumProvider = checksumProvider;
        }

        #endregion

        #region Functionality

        public ValueTask<ulong?> CalculateChecksumAsync() => _ChecksumProvider();

        public ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            return Content.CopyPooledAsync(target, bufferSize);
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
