using System;
using System.IO;

namespace GenHTTP.Modules.IO.Ranges
{

    public class RangedStream : Stream
    {
        private long _Pos = 0;

        #region Get-/Setters

        private Stream Target { get; }

        private long Start { get; }

        private long End { get; }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => (End - Start);

        public override long Position
        {
            get => (_Pos - Start > 0) ? (_Pos - Start) : 0;
            set => throw new NotSupportedException();
        }

        #endregion

        #region Initialization

        public RangedStream(Stream target, ulong start, ulong end)
        {
            Target = target;

            Start = (long)start;
            End = (long)end;
        }

        #endregion

        #region Functionality

        public override void Flush() => Target.Flush();

        public override void Write(byte[] buffer, int offset, int count)
        {
            long actualOffset = offset;
            long actualCount = count;

            if (_Pos < Start)
            {
                actualOffset += (int)(Start - _Pos);
                actualCount -= (int)(Start - _Pos);
            }

            if (_Pos + actualCount > End)
            {
                actualCount -= (int)(_Pos + actualCount) - End;
            }

            if (actualOffset < buffer.Length)
            {
                var toWrite = Math.Min(buffer.Length - actualOffset, actualCount);

                Target.Write(buffer, (int)actualOffset, (int)toWrite);
            }

            _Pos += count;
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        #endregion

    }

}
