using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GenHTTP.Modules.Core.General
{

    public class FilteredStream : Stream
    {
        private static ArrayPool<byte> POOL = ArrayPool<byte>.Shared;

        #region Get-/Setters

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => 0;

        public override long Position { get => 0; set => throw new NotSupportedException(); }

        public Stream InputStream { get; }

        public Stream ConversionStream { get; protected set; }

        public MemoryStream OutputStream { get; }

        #endregion

        #region Initialization

        public FilteredStream(Stream input, Func<Stream, Stream> linker)
        {
            OutputStream = new MemoryStream();

            InputStream = input;
            ConversionStream = linker(OutputStream);
        }

        #endregion

        #region Functionality

        public override int Read(byte[] buffer, int offset, int count)
        {
            // read from the input stream
            var inputBuffer = POOL.Rent(count);

            try
            {
                var read = InputStream.Read(inputBuffer, 0, inputBuffer.Length);

                // convert the data
                ConversionStream.Write(inputBuffer, 0, read);
                ConversionStream.Flush();

                // push the converted data into the given buffer
                OutputStream.Seek(0, SeekOrigin.Begin);
                var convertedRead = OutputStream.Read(buffer, offset, count);

                OutputStream.SetLength(0); // free memory

                return convertedRead;
            }
            finally
            {
                POOL.Return(inputBuffer);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                ConversionStream.Dispose();
                OutputStream.Dispose();
                InputStream.Dispose();
            }
        }

        #endregion

        #region Not supported

        public override void Flush()
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
            throw new NotSupportedException();
        }

        #endregion

    }

}
