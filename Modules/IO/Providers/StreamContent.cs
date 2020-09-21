using System;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.General
{

    public class StreamContent : IResponseContent, IDisposable
    {

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

        #endregion

        #region Initialization

        public StreamContent(Stream content)
        {
            Content = content;
        }

        #endregion

        #region Functionality

        public async Task Write(Stream target, uint bufferSize)
        {
            if (Content.CanSeek && (Content.Position != 0))
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
