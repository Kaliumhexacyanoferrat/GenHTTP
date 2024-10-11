using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Engine.Protocol;

/// <summary>
///     Implements chunked transfer encoding by letting the client
///     know how many bytes have been written to the response stream.
/// </summary>
/// <remarks>
///     Response streams are always wrapped into a chunked stream as
///     soon as there is no known content length. To avoid this overhead,
///     specify the length of your content whenever possible.
/// </remarks>
public sealed class ChunkedStream : Stream
{
    private static readonly string NL = "\r\n";

    #region Initialization

    public ChunkedStream(Stream target)
    {
        Target = target;
    }

    #endregion

    #region Get-/Setters

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    private Stream Target { get; }

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

            NL.Write(Target);
        }
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (count > 0)
        {
            await WriteAsync(count);

            await Target.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);

            await WriteAsync(NL);
        }
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (!buffer.IsEmpty)
        {
            await WriteAsync(buffer.Length);

            await Target.WriteAsync(buffer, cancellationToken);

            await WriteAsync(NL);
        }
    }

    public async ValueTask FinishAsync()
    {
        await WriteAsync("0\r\n\r\n");
    }

    public override void Flush()
    {
        Target.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken) => Target.FlushAsync(cancellationToken);

    private void Write(int value) => $"{value:X}\r\n".Write(Target);

    private ValueTask WriteAsync(string text) => text.WriteAsync(Target);

    private ValueTask WriteAsync(int value) => $"{value:X}\r\n".WriteAsync(Target);

    #endregion

}
