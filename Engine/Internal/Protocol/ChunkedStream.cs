using GenHTTP.Engine.Internal.Utilities;

namespace GenHTTP.Engine.Internal.Protocol;

/// <summary>
/// Implements chunked transfer encoding by letting the client
/// know how many bytes have been written to the response stream.
/// </summary>
/// <remarks>
/// Response streams are always wrapped into a chunked stream as
/// soon as there is no known content length. To avoid this overhead,
/// specify the length of your content whenever possible.
/// </remarks>
public sealed class ChunkedStream(PoolBufferedStream target) : Stream
{

    #region Get-/Setters

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    private Stream Target { get; } = target;

    #endregion

    #region Functionality

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (count > 0)
        {
            Write(count);

            Target.Write(buffer, offset, count);

            Target.Write("\r\n"u8);
        }
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (count > 0)
        {
            Write(count);

            await Target.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);

            Target.Write("\r\n"u8);
        }
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (!buffer.IsEmpty)
        {
            Write(buffer.Length);

            await Target.WriteAsync(buffer, cancellationToken);

            Target.Write("\r\n"u8);
        }
    }

    public void Finish()
    {
        Target.Write("0\r\n\r\n"u8);
    }

    public override void Flush()
    {
        Target.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken) => Target.FlushAsync(cancellationToken);

    private void Write(int value)
    {
        Span<byte> buffer = stackalloc byte[8 + 2];

        if (value.TryFormat(buffer, out var written, "X"))
        {
            buffer[written++] = (byte)'\r';
            buffer[written++] = (byte)'\n';

            Target.Write(buffer[..written]);
        }
        else
        {
            throw new InvalidOperationException("Failed to format chunk size");
        }
    }

    #endregion

}
