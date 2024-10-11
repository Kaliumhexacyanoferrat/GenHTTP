using System;
using System.IO;

namespace GenHTTP.Modules.IO.Ranges;

/// <summary>
/// A stream that can be configured to just write a specified
/// portion of the incoming data into the underlying stream.
/// </summary>
public class RangedStream : Stream
{

    #region Get-/Setters

    private Stream Target { get; }

    private long Start { get; }

    private long End { get; }

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => (End - Start);

    public override long Position { get; set; }

    #endregion

    #region Initialization

    /// <summary>
    /// Creates a ranged stream that writes the specified portion of
    /// data to the given target stream.
    /// </summary>
    /// <param name="target">The stream to write to</param>
    /// <param name="start">The zero based index of the starting position</param>
    /// <param name="end">The zero based index of the inclusive end position</param>
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
            if (Position > End) return;

            long actualOffset = offset;
            long actualCount = count;

            if (Position < Start)
            {
                actualOffset += (int)(Start - Position);
                actualCount -= (int)(Start - Position);
            }

            if (Start + actualCount > End + 1)
            {
                actualCount = Math.Min(End - Start + 1, actualCount);
            }

            if (actualOffset < buffer.Length)
            {
                var toWrite = Math.Min(buffer.Length - actualOffset, actualCount);

                Target.Write(buffer, (int)actualOffset, (int)toWrite);
            }

            Position += count;
        }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    #endregion

}
