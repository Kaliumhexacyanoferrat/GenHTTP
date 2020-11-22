using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using PooledAwait;

namespace GenHTTP.Engine.Protocol
{

    public sealed class ChunkedStream : Stream
    {
        private static readonly string NL = "\r\n";

        private static readonly Encoding ENCODING = Encoding.ASCII;

        private static readonly ArrayPool<byte> POOL = ArrayPool<byte>.Shared;

        #region Get-/Setters

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        private Stream Target { get; }

        #endregion

        #region Initialization

        public ChunkedStream(Stream target)
        {
            Target = target;
        }

        #endregion

        #region Functionality

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count > 0)
            {
                Write(count.ToString("X"));
                Write(NL);

                Target.Write(buffer, offset, count);
                
                Write(NL);
            }
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (count > 0)
            {
                await WriteAsync(count.ToString("X")).ConfigureAwait(false);
                await WriteAsync(NL);

                await Target.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);

                await WriteAsync(NL);
            }
        }

        public async PooledValueTask FinishAsync()
        {
            await WriteAsync("0").ConfigureAwait(false);
            await WriteAsync(NL);

            await WriteAsync(NL);
        }

        public override void Flush()
        {
            Target.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken) => Target.FlushAsync(cancellationToken);

        private void Write(string text)
        {
            var length = text.Length;

            var buffer = POOL.Rent(length);

            try
            {
                ENCODING.GetBytes(text, 0, length, buffer, 0);

                Target.Write(buffer, 0, length);
            }
            finally
            {
                POOL.Return(buffer);
            }
        }

        private async PooledValueTask WriteAsync(string text)
        {
            var length = text.Length;

            var buffer = POOL.Rent(length);

            try
            {
                ENCODING.GetBytes(text, 0, length, buffer, 0);

                await Target.WriteAsync(buffer.AsMemory(0, length));
            }
            finally
            {
                POOL.Return(buffer);
            }
        }

        #endregion

    }

}
