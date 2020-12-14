using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;

using PooledAwait;

namespace GenHTTP.Engine.Utilities
{

    public sealed class PoolBufferedStream : Stream
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

        public PoolBufferedStream(Stream stream)
        {
            Stream = stream;

            Buffer = POOL.Rent(8192);
            Current = 0;
        }

        #endregion

        #region Functionality

        public override int Read(byte[] buffer, int offset, int count) => Stream.Read(buffer, offset, count);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => Stream.ReadAsync(buffer, offset, count, cancellationToken);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => Stream.ReadAsync(buffer, cancellationToken);

        public override int Read(Span<byte> buffer) => Stream.Read(buffer);

        public override int ReadByte() => Stream.ReadByte();

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

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return DoWriteAsync(this, buffer, offset, count, cancellationToken);

            static async PooledTask DoWriteAsync(PoolBufferedStream self, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                if (count < (self.Buffer.Length - self.Current))
                {
                    System.Buffer.BlockCopy(buffer, offset, self.Buffer, self.Current, count);

                    self.Current += count;

                    if (self.Current == self.Buffer.Length)
                    {
                        await self.WriteBufferAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    await self.WriteBufferAsync(cancellationToken).ConfigureAwait(false);

                    await self.Stream.WriteAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public override void Flush()
        {
            WriteBuffer();

            Stream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return DoFlushAsync(this, cancellationToken);

            static async PooledTask DoFlushAsync(PoolBufferedStream self, CancellationToken cancellationToken)
            {
                await self.WriteBufferAsync(cancellationToken).ConfigureAwait(false);

                await self.Stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private void WriteBuffer()
        {
            if (Current > 0)
            {
                Stream.Write(Buffer, 0, Current);

                Current = 0;
            }
        }

        private async PooledValueTask WriteBufferAsync(CancellationToken cancellationToken)
        {
            if (Current > 0)
            {
                await Stream.WriteAsync(Buffer.AsMemory(0, Current), cancellationToken).ConfigureAwait(false);

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
