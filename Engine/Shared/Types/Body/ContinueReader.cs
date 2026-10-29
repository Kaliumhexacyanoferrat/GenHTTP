using System.Buffers;
using System.IO.Pipelines;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types.Body;

/// <summary>
/// Reads the body of a request whose client sent <c>Expect: 100-continue</c> and
/// holds the body back until it receives a 100 (Continue) interim response
/// (RFC 9110, section 10.1.1).
/// </summary>
/// <remarks>
/// The 100 is sent with the first read of the body, as Kestrel does, so a handler
/// that answers without reading the body (e.g. with a 401 or 413) never invites
/// the upload. Once the final response has started, the reader is disarmed, as no
/// interim response may follow a final one.
/// </remarks>
internal sealed class ContinueReader : PipeReader
{
    private static readonly ReadOnlyMemory<byte> Continue = "HTTP/1.1 100 Continue\r\n\r\n"u8.ToArray();

    private PipeReader? _reader;

    private PipeWriter? _writer;

    #region Get-/Setters

    /// <summary>
    /// Whether the 100 (Continue) has been sent to the client.
    /// </summary>
    internal bool Sent { get; private set; }

    private PipeReader Reader => _reader ?? throw new InvalidOperationException("Reader has not been initialized");

    #endregion

    #region Initialization

    /// <summary>
    /// Reads from the given reader and sends the 100 (Continue) to the given
    /// writer before the first read.
    /// </summary>
    internal void Arm(PipeReader reader, PipeWriter writer)
    {
        _reader = reader;
        _writer = writer;

        Sent = false;
    }

    /// <summary>
    /// Ensures that the 100 (Continue) will not be sent anymore.
    /// </summary>
    internal void Disarm() => _writer = null;

    internal void Reset()
    {
        _reader = null;
        _writer = null;

        Sent = false;
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Checks whether the given expectation asks for a 100 (Continue).
    /// </summary>
    internal static bool IsContinue(ByteString expectation) => Ascii.EqualsIgnoreCase(expectation.Bytes.Span, "100-continue"u8);

    public override ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        var writer = _writer;

        if (writer != null)
        {
            return ContinueAndReadAsync(writer, cancellationToken);
        }

        return Reader.ReadAsync(cancellationToken);
    }

    private async ValueTask<ReadResult> ContinueAndReadAsync(PipeWriter writer, CancellationToken cancellationToken)
    {
        _writer = null;

        Sent = true;

        writer.Write(Continue.Span);

        await writer.FlushAsync(cancellationToken);

        return await Reader.ReadAsync(cancellationToken);
    }

    public override bool TryRead(out ReadResult result) => Reader.TryRead(out result);

    public override void AdvanceTo(SequencePosition consumed) => Reader.AdvanceTo(consumed);

    public override void AdvanceTo(SequencePosition consumed, SequencePosition examined) => Reader.AdvanceTo(consumed, examined);

    public override void CancelPendingRead() => Reader.CancelPendingRead();

    public override void Complete(Exception? exception = null) => Reader.Complete(exception);

    #endregion

}
