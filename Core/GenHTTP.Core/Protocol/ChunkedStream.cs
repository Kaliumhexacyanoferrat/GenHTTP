using System;
using System.IO;
using System.Text;

namespace GenHTTP.Core.Protocol
{

    public class ChunkedStream : Stream
    {
        private static readonly string NL = "\r\n";

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
            var size = (count - offset);

            if (size > 0)
            {
                Write($"{size.ToString("X")}{NL}");
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
            var buffer = Encoding.ASCII.GetBytes(text);
            Target.Write(buffer, 0, buffer.Length);
        }

        #endregion

    }

}
