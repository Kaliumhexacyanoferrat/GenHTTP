using System.IO.Pipelines;

namespace GenHTTP.Adapters.AspNetCore.Context;

/// <summary>
/// A pipe writer over the response body that reports a client which went away as a completed
/// flush, the way a pipe reports a reader that is gone.
/// </summary>
/// <remarks>
/// Kestrel takes writes to an aborted response and drops them without a word; the only sign that
/// the client left is the request's abort token. Without this, a content that writes until a flush
/// says the connection is gone (a server-sent event source) would write for nobody forever.
/// </remarks>
internal sealed class AbortAwarePipeWriter(PipeWriter inner, CancellationToken aborted) : PipeWriter
{

    public override bool CanGetUnflushedBytes => inner.CanGetUnflushedBytes;

    public override long UnflushedBytes => inner.UnflushedBytes;

    public override void Advance(int bytes) => inner.Advance(bytes);

    public override Memory<byte> GetMemory(int sizeHint = 0) => inner.GetMemory(sizeHint);

    public override Span<byte> GetSpan(int sizeHint = 0) => inner.GetSpan(sizeHint);

    public override void CancelPendingFlush() => inner.CancelPendingFlush();

    public override void Complete(Exception? exception = null) => inner.Complete(exception);

    public override ValueTask CompleteAsync(Exception? exception = null) => inner.CompleteAsync(exception);

    public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
    {
        if (aborted.IsCancellationRequested)
        {
            return new(new FlushResult(isCanceled: false, isCompleted: true));
        }

        var pending = inner.FlushAsync(cancellationToken);

        if (pending.IsCompletedSuccessfully)
        {
            return new(Report(pending.Result));
        }

        return AwaitFlushAsync(pending);
    }

    private async ValueTask<FlushResult> AwaitFlushAsync(ValueTask<FlushResult> pending) => Report(await pending);

    private FlushResult Report(FlushResult result)
        => aborted.IsCancellationRequested ? new FlushResult(result.IsCanceled, isCompleted: true) : result;

}
