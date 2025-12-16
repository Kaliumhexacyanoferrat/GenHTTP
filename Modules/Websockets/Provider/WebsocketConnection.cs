using System.Buffers;
using System.Diagnostics.Contracts;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Websockets.Protocol;
using GenHTTP.Modules.Websockets.Utils;

namespace GenHTTP.Modules.Websockets.Provider;

// TODO: Evaluate using ObjectPool for WebsocketFrames to avoid allocating new objects (not for this PR)

public class WebsocketConnection : IReactiveConnection, IImperativeConnection, IAsyncDisposable
{
    private readonly bool _handleContinuationFramesManually;
    
    private readonly Stream _stream;
    private readonly PipeReader _pipeReader;
    private readonly int _rxMaxBufferSize;
    private SequencePosition _consumed;
    private SequencePosition _examined;
    private ReadOnlySequence<byte> _currentSequence;

    private readonly Queue<WebsocketFrame> _queuedFrames;
    
    
    #region Get-/Setters

    public IRequest Request { get; }

    #endregion

    #region Initialization

    public WebsocketConnection(IRequest request, Stream stream, int rxMaxBufferSize = 1024 * 16, bool handleContinuationFramesManually = true)
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
        _queuedFrames = new Queue<WebsocketFrame>();
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

    public ValueTask<WebsocketFrame> ReadFrameAsync(CancellationToken token = default)
    {
        /*if (_queuedFrames.TryDequeue(out var frame))
        {
            // There are frames in the queue, likely do to control frames received amid a segmented frame.
            return ValueTask.FromResult(frame);
        }*/
        
        if (_handleContinuationFramesManually)
        {
            return ReadFrameSegmentAsync(isFirstFrame: true, token: token);
        }
        else
        {
            return ReadSegmentedFrameAsync(token);
        }
    }

    private async ValueTask<WebsocketFrame> ReadSegmentedFrameAsync(CancellationToken token = default)
    {
        /*
         * Read the first frame
         * If the frame is FIN=1 just return the frame.
         * If the frame is not FIN=1 we are at a segmented case.
         */
        
        // Read a frame
        var frame = await ReadFrameSegmentAsync(isFirstFrame: true, token);
        
        // Hot Path, most frames will be single
        // If the frame is single, return it.
        if (frame.Fin)
        {
            return frame;
        }
        
        // Examine here to advance the pipe reader examined position,
        // if the frame was FIN=1, Consume() would have been called instead by the reactive loop/imperative logic
        Examine();
        
        // At this point if the received frame was Error or Control frame it would have already been returned as those have FIN=1
        // Meaning that we are at the presence of a Text or Binary frame
        
        // Cold path, read the rest of the continuation frames

        frame.IsSegmentedFrame = true;
        frame.SegmentedRawData = [ frame.RawData ];

        while (Request.Server.Running)
        {
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
                
                // Ping/Pong, queue them
                // We need to allocate here because when this frame is handled by the user,
                // the pipe reader internal buffer is already consumed
                
                // Note: Another possibility is to immediately return the ping/pong and handle the rest of the continuation frame after
                // but that raises a lot more complexity as we can't consume the pipe reader until the segmented frame is fully received
                // and would complicate the overall logic, specially for the imperative to which the user would need to handle extra cases.
                nextFrame.SetCachedData();
                _queuedFrames.Enqueue(nextFrame);
                Examine();
                continue;
                
                // This returned frame must signal that Examine must be called, not Consume, also we can't lose the frame
                // data, must store it and continue it, meaning that the first part of this method should be skipped!
                
                // In the ReactiveWebsocketContent, it should call a method that lives on the WebsocketConnection which 
                // is smart enough to know whether to examine or consume, also in the imperative case the user might 
                // only call examine so we should be able to keep slicing the new data @ReadFrameSegmentAsync 
            }
            
            frame.SegmentedRawData.Add(nextFrame.RawData);
            
            // If the received frame is FIN=1, merge all the SegmentedRawData into RawData
            if (nextFrame.Fin)
            {
                frame.RawData = SequenceUtils.ConcatSequences(frame.SegmentedRawData);

                return frame;
            }
            
            Examine();
        }

        return new WebsocketFrame(new FrameError("Unable to receive or assemble the segmented frame.", FrameErrorType.UndefinedBehavior));
    }

    /// <summary>
    /// Slice the read data to not include the already examined bytes.
    /// </summary>
    private async ValueTask<WebsocketFrame> ReadFrameSegmentAsync(bool isFirstFrame = true, CancellationToken token = default)
    {
        while (true)
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
                ? result.Buffer.Slice(_examined) 
                : result.Buffer;

            var frame = Frame.Decode(ref _currentSequence, _rxMaxBufferSize, out var consumed, out var examined);
            _consumed = consumed;
            _examined = examined;

            // Need more data for a complete frame
            if (frame.Type == FrameType.Error &&
                frame.FrameError!.ErrorType == FrameErrorType.Incomplete)
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
    }

    /// <summary>
    /// Advances examined data, marks the pipe reader we are ready for more data.
    /// </summary>
    private void Examine() => _pipeReader.AdvanceTo(_currentSequence.Start, _examined);
    
    /// <summary>
    /// Advances consumed and examined data, memory portion is no longer guaranteed to be valid.
    /// </summary>
    public void Consume() => _pipeReader.AdvanceTo(_consumed, _examined);
    
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
            FrameType.Text => false,
            FrameType.None => false,
            FrameType.Binary => false,
            FrameType.Continue => false,
            FrameType.Close => true,
            FrameType.Ping => true,
            FrameType.Pong => true,
            FrameType.Error => false,
            _ => throw new ArgumentOutOfRangeException($"Invalid WebsocketFrame Type {nameof(frame.Type)}")
        };
    }
    
    #endregion
    
    #region Disposable Pattern

    public async ValueTask DisposeAsync()
    {
        await _pipeReader.CompleteAsync();
    }

    #endregion

}