using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

using PooledAwait;

namespace GenHTTP.Modules.IO.Streaming
{

    public class StreamContent : IResponseContent, IDisposable
    {
        private static readonly ArrayPool<byte> POOL = ArrayPool<byte>.Shared;

        private readonly Func<ulong?> _ChecksumProvider;

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

        public Task Write(Stream target, uint bufferSize)
        {
            return DoWrite(this, target, bufferSize);

            static async PooledTask DoWrite(StreamContent self, Stream target, uint bufferSize)
            {
                if (self.Content.CanSeek && self.Content.Position != 0)
                {
                    self.Content.Seek(0, SeekOrigin.Begin);
                }

                var buffer = POOL.Rent((int)bufferSize);

                var memory = buffer.AsMemory();

                try
                {
                    int read;

                    do
                    {
                        read = await self.Content.ReadAsync(memory);

                        if (read > 0)
                        {
                            await target.WriteAsync(memory.Slice(0, read));
                        }
                    } 
                    while (read > 0);
                }
                finally
                {
                    POOL.Return(buffer);
                }
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
