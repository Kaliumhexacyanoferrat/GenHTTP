using System.Buffers;
using System.Diagnostics.Contracts;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Websockets.Protocol;
using GenHTTP.Modules.Websockets.Utils;

namespace GenHTTP.Modules.Websockets.Provider;

public class WebsocketConnection : IReactiveConnection, IImperativeConnection, IAsyncDisposable
{
    private readonly bool _handleContinuationFramesManually;
    private readonly bool _allocateFrameData;

    private readonly Stream _stream;
    private readonly PipeReader _pipeReader;
    private readonly int _rxMaxBufferSize;
    private SequencePosition _consumed;
    private SequencePosition _examined;
    private ReadOnlySequence<byte> _currentSequence;

    private bool _skipFrameInit;
    private bool _keepPipeReaderBufferValid;

    private WebsocketFrame _frame = null!;

    #region Get-/Setters

    public IRequest Request { get; }

    #endregion

    #region Initialization

    public WebsocketConnection(IRequest request, Stream stream, int rxMaxBufferSize, bool handleContinuationFramesManually, bool allocateFrameData)
    {
        Request = request;
        _stream = stream;
        _rxMaxBufferSize =  rxMaxBufferSize;
        _pipeReader = PipeReader.Create(stream,
            new StreamPipeReaderOptions(
                MemoryPool<byte>.Shared,
                leaveOpen: true,
                bufferSize: rxMaxBufferSize,
                minimumReadSize: Math.Min( rxMaxBufferSize / 4 , 1024 )));

        _handleContinuationFramesManually = handleContinuationFramesManually;
        _allocateFrameData = allocateFrameData;
    }

    #endregion

    #region Functionality

    public async ValueTask WriteAsync(ReadOnlyMemory<byte> payload, FrameType opcode = FrameType.Text, bool fin = true, CancellationToken token = default)
    {
        using var frameOwner = Frame.Encode(payload, opcode: (byte)opcode, fin);
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await _stream.WriteAsync(frameMemory, token);
        await _stream.FlushAsync(token);
    }

    public async ValueTask PingAsync(CancellationToken token = default)
    {
        using var frameOwner = Frame.EncodePing();
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await _stream.WriteAsync(frameMemory, token);
        await _stream.FlushAsync(token);
    }

    public async ValueTask PongAsync(ReadOnlyMemory<byte> payload, CancellationToken token = default)
    {
        using var frameOwner = Frame.EncodePong(payload);
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await _stream.WriteAsync(frameMemory, token);
        await _stream.FlushAsync(token);
    }

    public async ValueTask PongAsync(string payload, CancellationToken token = default)
    {
        await PongAsync(Encoding.UTF8.GetBytes(payload), token);
    }

    public async ValueTask PongAsync(CancellationToken token = default)
    {
        await PongAsync(ReadOnlyMemory<byte>.Empty, token);
    }

    public async ValueTask CloseAsync(string? reason = null, ushort statusCode = 1000, CancellationToken token = default)
    {
        using var frameOwner = Frame.EncodeClose(reason, statusCode);
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await _stream.WriteAsync(frameMemory, token);
        await _stream.FlushAsync(token);
    }

    public async ValueTask<IWebsocketFrame> ReadFrameAsync(CancellationToken token = default)
    {
        Advance();

        if (_handleContinuationFramesManually)
        {
            return await ReadFrameSegmentAsync(isFirstFrame: true, token: token);
        }

        return await ReadSegmentedFrameAsync(token);
    }

    private async ValueTask<WebsocketFrame> ReadSegmentedFrameAsync(CancellationToken token = default)
    {
        try
        {
            if (!_skipFrameInit) // When picking up an ongoing segmented frame after being interrupted by a Ping/Pong
            {
                _frame = await ReadFrameSegmentAsync(isFirstFrame: true, token);

                if (_frame.Fin) // Hot Path
                {
                    return _frame;
                }

                _frame.IsSegmented = true;
                _frame.Segments = [_frame.Memory];
            }
            else
            {
                _skipFrameInit = false;
            }

            // Cold path, segmented.
            while (Request.Server.Running)
            {
                Examine();

                // Read the next frame
                var nextFrame = await ReadFrameSegmentAsync(isFirstFrame: false, token);

                if (IsErrorFrame(nextFrame))
                {
                    // Return the error frame, disregard all the possible segment frames collected so far.
                    return nextFrame;
                }

                if (IsControlFrame(nextFrame))
                {
                    if (nextFrame.Type == FrameType.Close)
                    {
                        return nextFrame;
                    }

                    _keepPipeReaderBufferValid = true;
                    _skipFrameInit = true;
                    return nextFrame;
                }

                if (_frame.Segments is null) // This SHOULD be impossible to happen
                {
                    throw new ArgumentNullException(nameof(_frame.Segments));
                }

                _frame.Segments.Add(nextFrame.Memory);

                // If the received frame is FIN=1, merge all the SegmentedRawData into RawData
                if (nextFrame.Fin)
                {
                    _frame.Memory = SequenceUtils.ConcatSequences(_frame.Segments);

                    return _frame;
                }
            }

            return new WebsocketFrame(new FrameError("Unable to receive or assemble the segmented frame.",
                FrameErrorType.UndefinedBehavior));
        }
        finally
        {
            if (_allocateFrameData)
            {
                _frame.SetCachedData();
            }
        }
    }

    /// <summary>
    /// Slice the read data to not include the already examined bytes.
    /// </summary>
    private async ValueTask<WebsocketFrame> ReadFrameSegmentAsync(bool isFirstFrame = true, CancellationToken token = default)
    {
        // Aux var to solve issue when TCP fragmentation meets frame segmentation,
        // need to keep the original _examined to not slice further in case of TCP fragmentation
        var innerExamined = _examined;

        while (Request.Server.Running)
        {
            var result = await _pipeReader.ReadAsync(token);

            if (result.IsCanceled)
            {
                return new WebsocketFrame(
                    frameError: new FrameError(FrameError.ReadCanceled, FrameErrorType.Canceled));
            }

            if (result.Buffer.Length == 0 && result.IsCompleted) // Clean EOF: no more data, reader completed
            {
                _pipeReader.AdvanceTo(result.Buffer.End, result.Buffer.End);
                return new WebsocketFrame(FrameType.Close);
            }

            _currentSequence = !isFirstFrame
                ? result.Buffer.Slice(innerExamined)
                : result.Buffer;

            var frame = Frame.Decode(ref _currentSequence, _rxMaxBufferSize, out var consumed, out var examined);
            _consumed = consumed;
            _examined = examined;

            // Need more data for a complete frame
            if (frame.IsError(out var error) && error.ErrorType == FrameErrorType.Incomplete)
            {
                if (result.IsCompleted)
                {
                    // Advance the reader to the end, it is completed and there is no more data
                    // The partial data of the request is therefore discarded

                    // Dev note: Should we eventually still give the user the partial data received?
                    _pipeReader.AdvanceTo(_currentSequence.End, _currentSequence.End);

                    return new WebsocketFrame(
                        frameError: new FrameError(FrameError.UnexpectedEndOfStream, FrameErrorType.IncompleteForever));
                }

                // Advance examined portion to ensure the PipeReader reads new data in case of incomplete request.
                // Consumed should remain at buffer start until Consume() is called to keep data valid
                Examine();

                continue; // read more data
            }

            // Any other frame (including other errors)
            return frame;
        }

        throw new InvalidOperationException("Server has been stopped");
    }

    private void Examine()
    {
        _pipeReader.AdvanceTo(_currentSequence.Start, _examined);
    }

    private void Consume()
    {
        _pipeReader.AdvanceTo(_consumed, _examined);
    }

    private void Advance()
    {
        if (_keepPipeReaderBufferValid)
        {
            Examine();
            _keepPipeReaderBufferValid = false;
            return;
        }

        Consume();
    }

    #endregion

    #region Private Static Inline Helpers

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsErrorFrame(WebsocketFrame frame) => frame.Type == FrameType.Error;

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsControlFrame(WebsocketFrame frame)
    {
        return frame.Type switch
        {
            FrameType.Close => true,
            FrameType.Ping => true,
            FrameType.Pong => true,
            _ => false
        };
    }

    #endregion

    #region Disposable Pattern

    public ValueTask DisposeAsync() => _pipeReader.CompleteAsync();

    #endregion

}
