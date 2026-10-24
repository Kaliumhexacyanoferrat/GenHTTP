using System.Buffers;
using System.IO.Pipelines;

namespace GenHTTP.Engine.Shared.Types.Body;

/// <summary>
/// Reads the body of a request whose client sent <c>Expect: 100-continue</c> and waits for an
/// interim 100 (Continue) before sending it (RFC 9110, section 10.1.1).
/// </summary>
/// <remarks>
/// The 100 goes out with the first read of the body, the way Kestrel answers it: a handler that
/// responds without reading (a 401, a 413) never invites the upload at all. Once the final response
/// has started, the reader is disarmed - an interim response after a final one is not allowed.
/// </remarks>
internal sealed class ContinueReader : PipeReader
{
    private static readonly ReadOnlyMemory<byte> Continue = "HTTP/1.1 100 Continue\r\n\r\n"u8.ToArray();

    private PipeReader? _reader;

    private PipeWriter? _writer;

    // Whether the 100 went out.
    internal bool Answered { get; private set; }

    private PipeReader Reader => _reader ?? throw new InvalidOperationException("The reader has not been initialized");

    // Reads from the given reader, answering the expectation on the given writer before the first read.
    internal PipeReader Arm(PipeReader reader, PipeWriter writer)
    {
        _reader = reader;
        _writer = writer;

        return this;
    }

    // Reads on without ever sending the 100.
    internal void Disarm() => _writer = null;

    // Lets go of the connection, so a pooled request does not hold on to it.
    internal void Reset()
    {
        _reader = null;
        _writer = null;

        Answered = false;
    }

    public override ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (_writer is { } writer)
        {
            return ContinueAndReadAsync(writer, cancellationToken);
        }

        return Reader.ReadAsync(cancellationToken);
    }

    // The client sends nothing until it has the 100, so it has to be on the wire before the read.
    private async ValueTask<ReadResult> ContinueAndReadAsync(PipeWriter writer, CancellationToken cancellationToken)
    {
        _writer = null;
        Answered = true;

        writer.Write(Continue.Span);

        await writer.FlushAsync(cancellationToken);

        return await Reader.ReadAsync(cancellationToken);
    }

    public override bool TryRead(out ReadResult result) => Reader.TryRead(out result);

    public override void AdvanceTo(SequencePosition consumed) => Reader.AdvanceTo(consumed);

    public override void AdvanceTo(SequencePosition consumed, SequencePosition examined) => Reader.AdvanceTo(consumed, examined);

    public override void CancelPendingRead() => Reader.CancelPendingRead();

    public override void Complete(Exception? exception = null) => Reader.Complete(exception);

}
