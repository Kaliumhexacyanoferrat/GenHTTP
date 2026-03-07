using System.IO.Pipelines;
using System.Net.Sockets;

namespace GenHTTP.Engine.Internal.Infrastructure.Endpoints;

public sealed class SocketPipeReader : IAsyncDisposable
{
    private const int MaxOsReceiveSize = 65_536;

    private static readonly PipeOptions PipeOptions = new(
        minimumSegmentSize: MaxOsReceiveSize,
        pauseWriterThreshold: MaxOsReceiveSize * 2,
        resumeWriterThreshold: MaxOsReceiveSize,
        useSynchronizationContext: false
    );

    private readonly Socket _socket;
    private readonly Pipe _pipe;
    private readonly Task _fillTask;
    private readonly CancellationTokenSource _cts = new();

    public PipeReader Reader => _pipe.Reader;

    public SocketPipeReader(Socket socket)
    {
        _socket = socket ?? throw new ArgumentNullException(nameof(socket));
        _pipe = new Pipe(PipeOptions);
        _fillTask = FillPipeAsync(_cts.Token);
    }

    private async Task FillPipeAsync(CancellationToken ct)
    {
        var writer = _pipe.Writer;

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var memory = writer.GetMemory(MaxOsReceiveSize);

                var bytesRead = await _socket
                    .ReceiveAsync(memory, SocketFlags.None, ct)
                    .ConfigureAwait(false);

                if (bytesRead == 0)
                    break;

                writer.Advance(bytesRead);

                var result = await writer.FlushAsync(ct).ConfigureAwait(false);

                if (result.IsCompleted || result.IsCanceled)
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            /* normal shutdown */
        }
        catch (SocketException ex)
        {
            await writer.CompleteAsync(ex).ConfigureAwait(false);
            return;
        }
        catch (Exception ex)
        {
            await writer.CompleteAsync(ex).ConfigureAwait(false);
            return;
        }

        await writer.CompleteAsync().ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();

        try
        {
            await _fillTask.ConfigureAwait(false);
        }
        catch
        {
            /* already handled in FillPipeAsync */
        }

        await _pipe.Reader.CompleteAsync().ConfigureAwait(false);
        _cts.Dispose();
    }
}