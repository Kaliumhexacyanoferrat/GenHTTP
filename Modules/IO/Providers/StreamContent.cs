using System;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Providers
{

    public class StreamContent : IResponseContent, IDisposable
    {
        private readonly Func<ulong?> _ChecksumProvider;

        #region Get-/Setters

        private Stream Content { get; }

        public ulong? Length
        {
            get
            {
                if (Content.CanSeek && Content.Length > 0)
                {
                    return (ulong)Content.Length;
                }

                return null;
            }
        }

        public ulong? Checksum => _ChecksumProvider();

        #endregion

        #region Initialization

        public StreamContent(Stream content, Func<ulong?> checksumProvider)
        {
            Content = content;
            _ChecksumProvider = checksumProvider;
        }

        #endregion

        #region Functionality

        public async Task Write(Stream target, uint bufferSize)
        {
            if (Content.CanSeek && Content.Position != 0)
            {
                Content.Seek(0, SeekOrigin.Begin);
            }

            await Content.CopyToAsync(target, (int)bufferSize);
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
