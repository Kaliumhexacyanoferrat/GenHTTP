using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Core.Infrastructure.Endpoints
{

    internal class ReadAheadStream : Stream
    {
        private static ArrayPool<byte> POOL = ArrayPool<byte>.Shared;

        private int _Timeout;

        #region Get-/Setters

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override int ReadTimeout
        {
            get { return _Timeout; }
            set { _Timeout = value; }
        }

        internal MemoryStream Buffer { get; }

        private Stream BaseStream { get; }
        
        #endregion

        #region Initialization

        internal ReadAheadStream(Stream baseStream)
        {
            BaseStream = baseStream;
            Buffer = new MemoryStream();
        }

        #endregion

        #region Functionality

        public override void Flush()
        {
            BaseStream.Flush();
        }

        internal bool Peek()
        {
            var buffer = POOL.Rent(RequestParser.READ_BUFFER_SIZE);

            try
            {
                var read = BaseStream.Read(buffer, 0, buffer.Length);

                if (read > 0)
                {
                    Buffer.Write(buffer, 0, read);
                    Buffer.Seek(0, SeekOrigin.Begin);

                    return true;
                }

                return false;
            }
            finally
            {
                POOL.Return(buffer);
            }
        }

        [DebuggerHidden]
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Buffer.Position < Buffer.Length)
            {
                return Buffer.Read(buffer, offset, count);
            }
            else
            {
                Buffer.SetLength(0);
            }

            return BaseStream.Read(buffer, offset, count);
        }

        [DebuggerHidden]
        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            
            if (disposing)
            {
                BaseStream.Dispose();
            }
        }

        #endregion

        #region Not Supported

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        #endregion

    }

}
