using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace GenHTTP.Engine.Protocol
{

    public class ChunkedStream : Stream
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
                Write($"{count.ToString("X")}{NL}");
                Target.Write(buffer, offset, count);
                Write(NL);
            }
        }

        public void Finish()
        {
            Write($"0{NL}{NL}");
        }

        public override void Flush()
        {
            Target.Flush();
        }

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

        #endregion

    }

}
