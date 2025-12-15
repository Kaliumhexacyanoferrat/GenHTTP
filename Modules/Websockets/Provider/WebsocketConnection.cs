using System.Buffers;
using System.Diagnostics.Contracts;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets.Provider;

// TODO: Evaluate using ObjectPool for WebsocketFrames to avoid allocating new objects

public class WebsocketConnection : IReactiveConnection, IImperativeConnection, IAsyncDisposable
{
    private bool _handleContinuationFramesAutomatically = false;
    
    private readonly Stream _stream;
    private readonly PipeReader _pipeReader;
    private readonly int _rxMaxBufferSize;
    private SequencePosition _consumed;
    private SequencePosition _examined;
    private ReadOnlySequence<byte> _currentSequence;
    
    
    #region Get-/Setters

    public IRequest Request { get; }

    #endregion

    #region Initialization

    public WebsocketConnection(IRequest request, Stream stream, int rxMaxBufferSize = 8192)
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
        if (!_handleContinuationFramesAutomatically)
        {
            return ReadFirstSegmentFrameAsync(token);
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
        var frame = await ReadNextSegmentFrameAsync(isFirstFrame: true, token);
        
        // Hot Path, most frames will be single
        // If the frame is single, return it.
        if (frame.Fin)
        {
            return frame;
        }
        
        // At this point if the received frame was Error or Control frame it would have already been returned as those have FIN=1
        // Meaning that we are at the presence of a Text or Binary frame
        
        // Cold path, read the rest of the continuation frames

        frame.IsSegmentedFrame = true;
        frame.SegmentedRawData = [ frame.RawData ];

        while (Request.Server.Running)
        {
            // Read the next frame
            var nextFrame = await ReadNextSegmentFrameAsync(isFirstFrame: false, token);

            if (IsErrorFrame(nextFrame))
            {
                // Return the error frame, disregard all the possible segment frames collected so far.
                return nextFrame;
            }

            if (IsControlFrame(nextFrame))
            {
                // It is possible to receive control frames in between segmented frames.
                // One approach could be to store this frame and handle it after the segmented frame is deal with.
            }

            // Given that the nextFrame is not a control frame, its type must match the initial frame's
            if (frame.Type != nextFrame.Type)
            {
                return new WebsocketFrame(new FrameError("Invalid continuation frame", FrameErrorType.InvalidContinuationFrame));
            }
            
            frame.SegmentedRawData.Add(nextFrame.RawData);
            
            // If the received frame is FIN=1, merge all the SegmentedRawData into RawData
            if (frame.Fin)
            {
                frame.RawData = SequenceUtils.ConcatSequences(frame.SegmentedRawData);

                return frame;
            }
        }

        return new WebsocketFrame(new FrameError("Unable to receive or assemble the segmented frame.", FrameErrorType.UndefinedBehavior));
    }
    
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

    /// <summary>
    /// Slice the read data to not include the already examined bytes.
    /// </summary>
    private async ValueTask<WebsocketFrame> ReadNextSegmentFrameAsync(bool isFirstFrame = false, CancellationToken token = default)
    {
        while (true)
        {
            var result = await _pipeReader.ReadAsync(token);
            
            if(!isFirstFrame)
                _currentSequence = result.Buffer.Slice(_examined);

            if (result.IsCanceled)
            {
                return new WebsocketFrame(
                    frameError: new FrameError(FrameError.ReadCanceled, FrameErrorType.Canceled));
            }

            if (_currentSequence.Length == 0 && result.IsCompleted) // Clean EOF: no more data, reader completed
            {
                _pipeReader.AdvanceTo(_currentSequence.End, _currentSequence.End);
                return new WebsocketFrame(FrameType.Close);
            }

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
    /// Wait for a frame to be acquired.
    /// </summary> 
    private async ValueTask<WebsocketFrame> ReadFirstSegmentFrameAsync(CancellationToken token = default)
    {
        while (true)
        {
            var result = await _pipeReader.ReadAsync(token);
            _currentSequence = result.Buffer;

            if (result.IsCanceled)
            {
                return new WebsocketFrame(
                    frameError: new FrameError(FrameError.ReadCanceled, FrameErrorType.Canceled));
            }

            if (_currentSequence.Length == 0 && result.IsCompleted) // Clean EOF: no more data, reader completed
            {
                _pipeReader.AdvanceTo(_currentSequence.End, _currentSequence.End);
                return new WebsocketFrame(FrameType.Close);
            }

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

                continue;  // read more data
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
    

    public async ValueTask DisposeAsync()
    {
        await _pipeReader.CompleteAsync();
    }

    #endregion

}

public sealed class BufferSegment : ReadOnlySequenceSegment<byte>
{
    public BufferSegment(ReadOnlyMemory<byte> memory) => Memory = memory;

    public BufferSegment Append(ReadOnlyMemory<byte> memory)
    {
        var next = new BufferSegment(memory)
        {
            RunningIndex = RunningIndex + Memory.Length
        };
        Next = next;
        return next;
    }
}

public static class SequenceUtils
{
    public static ReadOnlySequence<byte> ConcatSequences(List<ReadOnlySequence<byte>?>? sequences)
    {
        if (sequences is null || sequences.Count == 0)
            return ReadOnlySequence<byte>.Empty;

        BufferSegment? first = null;
        BufferSegment? last  = null;

        // for-loop is slightly faster than foreach on List<T>
        for (var i = 0; i < sequences.Count; i++)
        {
            var seqNullable = sequences[i];
            if (!seqNullable.HasValue)
                continue;

            var seq = seqNullable.Value;

            foreach (var mem in seq)
            {
                if (mem.Length == 0)
                    continue;

                if (first is null)
                {
                    first = last = new BufferSegment(mem);
                }
                else
                {
                    last = last!.Append(mem);
                }
            }
        }

        return first is null
            ? ReadOnlySequence<byte>.Empty
            : new ReadOnlySequence<byte>(first, 0, last!, last!.Memory.Length);
    }
    
    public static ReadOnlySequence<byte> ConcatSequences(ReadOnlySequence<byte>[] sequences)
    {
        BufferSegment? first = null;
        BufferSegment? last = null;

        foreach (var seq in sequences)
        {
            foreach (var mem in seq) // mem is ReadOnlyMemory<byte> for each segment
            {
                if (mem.Length == 0) continue;

                if (first is null)
                    first = last = new BufferSegment(mem);
                else
                    last = last!.Append(mem);
            }
        }

        if (first is null)
            return ReadOnlySequence<byte>.Empty;

        return new ReadOnlySequence<byte>(first, 0, last!, last!.Memory.Length);
    }
    
    // Hot path: concat only 2
    public static ReadOnlySequence<byte> ConcatTwo(in ReadOnlySequence<byte> a, in ReadOnlySequence<byte> b)
    {
        if (a.IsEmpty) return b;
        if (b.IsEmpty) return a;

        // Fast path: both single-segment.
        if (a.IsSingleSegment && b.IsSingleSegment)
        {
            var head = new BufferSegment(a.First);
            var tail = head.Append(b.First);
            return new ReadOnlySequence<byte>(head, 0, tail, tail.Memory.Length);
        }

        BufferSegment? headSeg = null;
        BufferSegment? tailSeg = null;

        AppendSequence(a, ref headSeg, ref tailSeg);
        AppendSequence(b, ref headSeg, ref tailSeg);

        if (headSeg is null) return ReadOnlySequence<byte>.Empty;

        return new ReadOnlySequence<byte>(headSeg, 0, tailSeg!, tailSeg!.Memory.Length);
    }

    private static void AppendSequence(in ReadOnlySequence<byte> seq, ref BufferSegment? head, ref BufferSegment? tail)
    {
        // foreach over ReadOnlySequence<byte> uses the struct enumerator; no allocations.
        foreach (var mem in seq)
        {
            if (mem.Length == 0) continue;

            if (head is null)
            {
                head = tail = new BufferSegment(mem);
            }
            else
            {
                tail = tail!.Append(mem);
            }
        }
    }
}