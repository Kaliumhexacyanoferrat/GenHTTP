using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Engine.Protocol
{

    /// <summary>
    /// Implements chunked transfer encoding by letting the client
    /// know how many bytes have been written to the response stream. 
    /// </summary>
    /// <remarks>
    /// Response streams are always wrapped into a chunked stream as
    /// soon as there is no known content length. To avoid this overhead,
    /// specify the length of your content whenever possible.
    /// </remarks>
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
                Write(count);
                Write(NL);

                Target.Write(buffer, offset, count);
                
                Write(NL);
            }
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (count > 0)
            {
                await WriteAsync(count).ConfigureAwait(false);
                await WriteAsync(NL);

                await Target.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);

                await WriteAsync(NL);
            }
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (!buffer.IsEmpty)
            {
                await WriteAsync(buffer.Length).ConfigureAwait(false);
                await WriteAsync(NL);

                await Target.WriteAsync(buffer, cancellationToken);

                await WriteAsync(NL);
            }
        }

        public async ValueTask FinishAsync()
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

        private void Write(int value) => Write($"{value:X}");

        private ValueTask WriteAsync(string text) => text.WriteAsync(Target);

        private ValueTask WriteAsync(int value) => WriteAsync($"{value:X}");

        #endregion

    }

}
