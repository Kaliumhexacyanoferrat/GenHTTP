using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GenHTTP.Engine.Utilities
{

    public class PoolBufferedStream : Stream
    {
        private static readonly ArrayPool<byte> POOL = ArrayPool<byte>.Shared;

        #region Get-/Setters

        public override bool CanRead => Stream.CanRead;

        public override bool CanSeek => Stream.CanSeek;

        public override bool CanWrite => Stream.CanWrite;

        public override long Length => Stream.Length;

        public override long Position
        {
            get => Stream.Position;
            set => Stream.Position = value;
        }

        private Stream Stream { get; }

        private byte[] Buffer { get; }

        private int Current { get; set; }

        #endregion

        #region Initialization

        public PoolBufferedStream(Stream stream, uint bufferSize)
        {
            Stream = stream;

            Buffer = POOL.Rent((int)bufferSize);
            Current = 0;
        }

        #endregion

        #region Functionality

        public override int Read(byte[] buffer, int offset, int count) => Stream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => Stream.Seek(offset, origin);

        public override void SetLength(long value) => Stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count < (Buffer.Length - Current))
            {
                System.Buffer.BlockCopy(buffer, offset, Buffer, Current, count);

                Current += count;

                if (Current == Buffer.Length)
                {
                    WriteBuffer();
                }
            }
            else
            {
                WriteBuffer();

                Stream.Write(buffer, offset, count);
            }
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (count < (Buffer.Length - Current))
            {
                System.Buffer.BlockCopy(buffer, offset, Buffer, Current, count);

                Current += count;

                if (Current == Buffer.Length)
                {
                    await WriteBufferAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                await WriteBufferAsync(cancellationToken).ConfigureAwait(false);

                await Stream.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            }
        }

        public override void Flush()
        {
            WriteBuffer();

            Stream.Flush();
        }

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            await WriteBufferAsync(cancellationToken).ConfigureAwait(false);

            await Stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        private void WriteBuffer()
        {
            if (Current > 0)
            {
                Stream.Write(Buffer, 0, Current);

                Current = 0;
            }
        }

        private async ValueTask WriteBufferAsync(CancellationToken cancellationToken)
        {
            if (Current > 0)
            {
                await Stream.WriteAsync(Buffer, 0, Current, cancellationToken).ConfigureAwait(false);

                Current = 0;
            }
        }

        #endregion

        #region Disposing

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    Stream.Dispose();
                }
                finally
                {
                    POOL.Return(Buffer);
                }
            }

            base.Dispose(disposing);
        }

        #endregion

    }

}
