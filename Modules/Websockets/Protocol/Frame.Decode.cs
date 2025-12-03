using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Text;

namespace GenHTTP.Modules.Websockets.Protocol;

public static partial class Frame
{
    private const string IncompleteFrame = "Incomplete frame";
    private const string InvalidOpCode = "Invalid OpCode";
    private const string InvalidControlFrame = "Invalid Control Frame";
    private const string InvalidControlFrameLength = "Invalid Control Frame Length";
    private const string PayloadTooLarge = "Payload is too large";

    /* Websockets RFC 6455 Frame Decode definition (LLM generated)

       The decode algorithm reads a raw WebSocket frame from a buffer and converts it into a structured WebsocketFrame by interpreting
       the bits according to RFC 6455. It begins by ensuring at least the minimum 2-byte header is present, then extracts the FIN bit and opcode,
       which determine whether the frame is a complete message, a fragment, or a control frame such as Ping, Pong, or Close. It then checks the
       MASK bit and the 7-bit base payload length. If the length field indicates an extended payload (126 or 127), the algorithm reads the
       appropriate 16-bit or 64-bit length field and adjusts the header size accordingly. Control-frame rules are enforced: control frames must
       not be fragmented and must not use extended lengths. After determining the final payload length and verifying that the buffer contains
       enough bytes for the full header, mask key, and payload, the decoder extracts the optional 4-byte mask key and applies the unmasking XOR
       operation in place if masking is enabled. For normal data frames (Text, Binary, Continue), the raw unmasked payload bytes are returned
       as-is, while Close frames are additionally parsed to extract the close code and UTF-8 reason. Finally, the method returns a WebsocketFrame
       containing the payload, frame type, and FIN flag, leaving higher-level logic to handle message reassembly and continuation semantics.
     */
    
    public static WebsocketFrame Decode2(
        ref ReadResult result,
        int rxMaxBufferSize,
        out SequencePosition consumed,
        out SequencePosition examined)
    {
        var buffer = result.Buffer;
        var reader = new SequenceReader<byte>(buffer);

        // Local tracking of what we actually want to report
        var localConsumed = buffer.Start;
        var localExamined = buffer.End;

        // Helper to return an "incomplete" error frame
        WebsocketFrame Incomplete()
        {
            // Don't consume anything, but we've examined everything we got
            localConsumed = buffer.Start;
            localExamined = buffer.End;

            return new WebsocketFrame(
                ReadOnlyMemory<byte>.Empty,
                Type: FrameType.Error,
                FrameError: new FrameError(IncompleteFrame, FrameErrorType.Incomplete));
        }

        // We need at least 2 bytes for b0 + b1
        if (reader.Remaining < 2)
        {
            var frame = Incomplete();
            consumed = localConsumed;
            examined = localExamined;
            return frame;
        }

        reader.TryRead(out byte b0);
        reader.TryRead(out byte b1);

        var fin = (b0 & 0b1000_0000) != 0;
        var opcode = (byte)(b0 & 0x0F);

        var frameType = opcode switch
        {
            0x00 => FrameType.Continue,
            0x01 => FrameType.Text,
            0x02 => FrameType.Binary,
            0x08 => FrameType.Close,
            0x09 => FrameType.Ping,
            0x0A => FrameType.Pong,
    #pragma warning disable S3928
            _ => FrameType.None
    #pragma warning restore S3928
        };

        if (frameType == FrameType.None)
        {
            // Invalid opcode: consume this byte pair and fail
            localConsumed = reader.Position;
            localExamined = reader.Position;

            var invalid = new WebsocketFrame(
                ReadOnlyMemory<byte>.Empty,
                Type: FrameType.Error,
                FrameError: new FrameError(InvalidOpCode, FrameErrorType.InvalidOpCode));

            consumed = localConsumed;
            examined = localExamined;
            return invalid;
        }

        var isControlFrame = frameType is FrameType.Close or FrameType.Ping or FrameType.Pong;
        var isMasked = (b1 & 0x80) != 0;
        var payloadLen7 = (byte)(b1 & 0x7F);

        // RFC: control frames MUST NOT be fragmented
        if (isControlFrame && !fin)
        {
            localConsumed = reader.Position;
            localExamined = reader.Position;

            var err = new WebsocketFrame(
                ReadOnlyMemory<byte>.Empty,
                Type: FrameType.Error,
                FrameError: new FrameError(InvalidControlFrame, FrameErrorType.InvalidControlFrame));

            consumed = localConsumed;
            examined = localExamined;
            return err;
        }

        // RFC: control frames MUST have payload length <= 125
        if (isControlFrame && payloadLen7 >= 126)
        {
            localConsumed = reader.Position;
            localExamined = reader.Position;

            var err = new WebsocketFrame(
                ReadOnlyMemory<byte>.Empty,
                Type: FrameType.Error,
                FrameError: new FrameError(InvalidControlFrameLength, FrameErrorType.InvalidControlFrameLength));

            consumed = localConsumed;
            examined = localExamined;
            return err;
        }

        long payloadLen64 = payloadLen7;

        // Extended lengths
        if (payloadLen7 == 126)
        {
            if (reader.Remaining < 2)
            {
                var frame = Incomplete();
                consumed = localConsumed;
                examined = localExamined;
                return frame;
            }

            reader.TryReadBigEndian(out short len16);
            payloadLen64 = len16;
        }
        else if (payloadLen7 == 127)
        {
            if (reader.Remaining < 8)
            {
                var frame = Incomplete();
                consumed = localConsumed;
                examined = localExamined;
                return frame;
            }

            reader.TryReadBigEndian(out long len64);
            payloadLen64 = len64;
        }
        
        // Maximum possible header size
        const int MaxFrameHeaderSize = 14;
        var maxAllowedPayload = rxMaxBufferSize - MaxFrameHeaderSize;
        
        // Payload too large, larger than the pipe reader internal buffer
        if (payloadLen64 > maxAllowedPayload)
        {
            localConsumed = reader.Position;
            localExamined = reader.Position;

            var err = new WebsocketFrame(
                ReadOnlyMemory<byte>.Empty,
                Type: FrameType.Error,
                FrameError: new FrameError(PayloadTooLarge, FrameErrorType.PayloadTooLarge));

            consumed = localConsumed;
            examined = localExamined;
            return err;
        }
        
        // max payload shouldn't be more than 32 bit - max pipe reader buffer size?
        var payloadLength = (int)payloadLen64;

        // Mask key (if present)
        Span<byte> maskKeySpan = stackalloc byte[4];
        if (isMasked)
        {
            if (reader.Remaining < 4 || !reader.TryCopyTo(maskKeySpan))
            {
                var frame = Incomplete();
                consumed = localConsumed;
                examined = localExamined;
                return frame;
            }

            reader.Advance(4);
        }

        // Now we need the full payload
        if (reader.Remaining < payloadLength)
        {
            var frame = Incomplete();
            consumed = localConsumed;
            examined = localExamined;
            return frame;
        }

        // Copy payload into our own buffer (don't mutate PipeReader's memory)
        var payloadArray = new byte[payloadLength];
        reader.TryCopyTo(payloadArray);
        reader.Advance(payloadLength);
        
        if (isMasked)
        {
            for (int i = 0; i < payloadArray.Length; i++)
            {
                payloadArray[i] ^= maskKeySpan[i & 0b11];
            }
        }

        ReadOnlyMemory<byte> payloadMem;

        if (frameType == FrameType.Close)
        {
            if (payloadArray.Length >= 2)
            {
                var closeCode = BinaryPrimitives.ReadUInt16BigEndian(payloadArray.AsSpan(0, 2));
                var reason = payloadArray.Length > 2
                    ? Encoding.UTF8.GetString(payloadArray.AsSpan(2))
                    : null;

                var msg = $"Close frame received. Code: {closeCode}, Reason: {reason ?? "None"}";
                payloadMem = Encoding.UTF8.GetBytes(msg).AsMemory();
            }
            else
            {
                payloadMem = ReadOnlyMemory<byte>.Empty;
            }
        }
        else
        {
            payloadMem = payloadArray.AsMemory();
        }

        // Successfully parsed one full frame.
        // Update the PipeReader with consumed + examined exactly up to this point.
        localConsumed = reader.Position;
        localExamined = reader.Position;

        consumed = localConsumed;
        examined = localExamined;

        return new WebsocketFrame(
            payloadMem,
            frameType,
            Fin: fin);
    }

    public static WebsocketFrame Decode(
        ref ReadResult result,
        int rxMaxBufferSize,
        out SequencePosition consumed,
        out SequencePosition examined)
    {
        var buffer = result.Buffer;
        var reader = new SequenceReader<byte>(buffer);

        // Local tracking of what we actually want to report
        consumed = buffer.Start;
        examined = buffer.End;

        // Helper to return an "incomplete" error frame
        WebsocketFrame Incomplete(ref SequencePosition consumed, ref SequencePosition examined)
        {
            // Don't consume anything, but we've examined everything we got
            consumed = buffer.Start;
            examined = buffer.End;

            return new WebsocketFrame(
                ReadOnlyMemory<byte>.Empty,
                Type: FrameType.Error,
                FrameError: new FrameError(IncompleteFrame, FrameErrorType.Incomplete));
        }

        // We need at least 2 bytes for b0 + b1
        if (reader.Remaining < 2)
        {
            var frame = Incomplete(ref consumed, ref examined);
            return frame;
        }

        reader.TryRead(out byte b0);
        reader.TryRead(out byte b1);

        var fin = (b0 & 0b1000_0000) != 0;
        var opcode = (byte)(b0 & 0x0F);

        var frameType = opcode switch
        {
            0x00 => FrameType.Continue,
            0x01 => FrameType.Text,
            0x02 => FrameType.Binary,
            0x08 => FrameType.Close,
            0x09 => FrameType.Ping,
            0x0A => FrameType.Pong,
#pragma warning disable S3928
            _ => FrameType.None
#pragma warning restore S3928
        };

        if (frameType == FrameType.None)
        {
            // Invalid opcode: consume this byte pair and fail
            consumed = reader.Position;
            examined = reader.Position;

            var invalid = new WebsocketFrame(
                ReadOnlyMemory<byte>.Empty,
                Type: FrameType.Error,
                FrameError: new FrameError(InvalidOpCode, FrameErrorType.InvalidOpCode));

            return invalid;
        }

        var isControlFrame = frameType is FrameType.Close or FrameType.Ping or FrameType.Pong;
        var isMasked = (b1 & 0x80) != 0;
        var payloadLen7 = (byte)(b1 & 0x7F);

        // RFC: control frames MUST NOT be fragmented
        if (isControlFrame && !fin)
        {
            consumed = reader.Position;
            examined = reader.Position;

            var err = new WebsocketFrame(
                ReadOnlyMemory<byte>.Empty,
                Type: FrameType.Error,
                FrameError: new FrameError(InvalidControlFrame, FrameErrorType.InvalidControlFrame));

            return err;
        }

        // RFC: control frames MUST have payload length <= 125
        if (isControlFrame && payloadLen7 >= 126)
        {
            consumed = reader.Position;
            examined = reader.Position;

            var err = new WebsocketFrame(
                ReadOnlyMemory<byte>.Empty,
                Type: FrameType.Error,
                FrameError: new FrameError(InvalidControlFrameLength, FrameErrorType.InvalidControlFrameLength));

            return err;
        }

        long payloadLen64 = payloadLen7;

        // Extended lengths
        if (payloadLen7 == 126)
        {
            if (reader.Remaining < 2)
            {
                var frame = Incomplete(ref consumed, ref examined);
                return frame;
            }

            reader.TryReadBigEndian(out short len16);
            payloadLen64 = len16;
        }
        else if (payloadLen7 == 127)
        {
            if (reader.Remaining < 8)
            {
                var frame = Incomplete(ref consumed, ref examined);
                return frame;
            }

            reader.TryReadBigEndian(out long len64);
            payloadLen64 = len64;
        }

        // Maximum possible header size
        const int MaxFrameHeaderSize = 14;
        var maxAllowedPayload = rxMaxBufferSize - MaxFrameHeaderSize;

        // Payload too large, larger than the pipe reader internal buffer
        if (payloadLen64 > maxAllowedPayload)
        {
            consumed = reader.Position;
            examined = reader.Position;

            var err = new WebsocketFrame(
                ReadOnlyMemory<byte>.Empty,
                Type: FrameType.Error,
                FrameError: new FrameError(PayloadTooLarge, FrameErrorType.PayloadTooLarge));

            return err;
        }

        // max payload shouldn't be more than 32 bit - max pipe reader buffer size?
        var payloadLength = (int)payloadLen64;

        // Mask key (if present)
        Span<byte> maskKeySpan = stackalloc byte[4];
        if (isMasked)
        {
            if (reader.Remaining < 4 || !reader.TryCopyTo(maskKeySpan))
            {
                var frame = Incomplete(ref consumed, ref examined);
                return frame;
            }

            reader.Advance(4);
        }

        // Now we need the full payload
        if (reader.Remaining < payloadLength)
        {
            var frame = Incomplete(ref consumed, ref examined);
            return frame;
        }

        // Copy payload into our own buffer (don't mutate PipeReader's memory)
        var payloadArray = new byte[payloadLength];
        reader.TryCopyTo(payloadArray);
        reader.Advance(payloadLength);

        if (isMasked)
        {
            for (int i = 0; i < payloadArray.Length; i++)
            {
                payloadArray[i] ^= maskKeySpan[i & 0b11];
            }
        }

        ReadOnlyMemory<byte> payloadMem;

        if (frameType == FrameType.Close)
        {
            if (payloadArray.Length >= 2)
            {
                var closeCode = BinaryPrimitives.ReadUInt16BigEndian(payloadArray.AsSpan(0, 2));
                var reason = payloadArray.Length > 2
                    ? Encoding.UTF8.GetString(payloadArray.AsSpan(2))
                    : null;

                var msg = $"Close frame received. Code: {closeCode}, Reason: {reason ?? "None"}";
                payloadMem = Encoding.UTF8.GetBytes(msg).AsMemory();
            }
            else
            {
                payloadMem = ReadOnlyMemory<byte>.Empty;
            }
        }
        else
        {
            payloadMem = payloadArray.AsMemory();
        }

        // Successfully parsed one full frame.
        // Update the PipeReader with consumed + examined exactly up to this point.
        consumed = reader.Position;
        examined = reader.Position;

        return new WebsocketFrame(
            payloadMem,
            frameType,
            Fin: fin);
    }

    [Obsolete]
    public static WebsocketFrame Decode(Memory<byte> buffer, int length)
    {
        var span = buffer.Span;

        if (length < 2)
        {
            return new WebsocketFrame(
                ReadOnlyMemory<byte>.Empty,
                Type: FrameType.Error,
                FrameError: new FrameError(IncompleteFrame, FrameErrorType.Incomplete));
        }

        var b0 = span[0];
        var b1 = span[1];

        // FIN + opcode
        var fin = (b0 & 0b1000_0000) != 0;
        var opcode = b0 & 0x0F;

        // Map opcode to frame type
        var frameType = opcode switch
        {
            0x00 => FrameType.Continue,
            0x01 => FrameType.Text,
            0x02 => FrameType.Binary,
            0x08 => FrameType.Close,
            0x09 => FrameType.Ping,
            0x0A => FrameType.Pong,
    #pragma warning disable S3928
            _ => FrameType.None
    #pragma warning restore S3928
        };

        if (frameType == FrameType.None)
        {
            return new WebsocketFrame(
                ReadOnlyMemory<byte>.Empty,
                Type: FrameType.Error,
                FrameError: new FrameError(InvalidOpCode, FrameErrorType.InvalidOpCode));
        }

        var isControlFrame = frameType is FrameType.Close or FrameType.Ping or FrameType.Pong;

        // MASK bit
        var isMasked = (b1 & 0x80) != 0;

        // 7-bit payload length
        var payloadLen7 = b1 & 0x7F;
        var payloadStart = 2;
        var payloadLength64 = (ulong)payloadLen7;

        switch (isControlFrame)
        {
            // RFC 6455: control frames MUST NOT be fragmented
            case true when !fin:
                return new WebsocketFrame(
                    ReadOnlyMemory<byte>.Empty,
                    Type: FrameType.Error,
                    FrameError: new FrameError(InvalidControlFrame, FrameErrorType.InvalidControlFrame));
            // RFC 6455: control frames MUST have payload length <= 125
            case true when payloadLen7 >= 126:
                return new WebsocketFrame(
                    ReadOnlyMemory<byte>.Empty,
                    Type: FrameType.Error,
                    FrameError: new FrameError(InvalidControlFrameLength, FrameErrorType.InvalidControlFrameLength));
        }

        switch (payloadLen7)
        {
            // Extended payload lengths
            case 126 when length < 4:
                return new WebsocketFrame(
                    ReadOnlyMemory<byte>.Empty,
                    Type: FrameType.Error,
                    FrameError: new FrameError(IncompleteFrame, FrameErrorType.Incomplete));
            case 126:
                payloadLength64 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(2, 2));
                payloadStart = 4;
                break;
            case 127 when length < 10:
                return new WebsocketFrame(
                    ReadOnlyMemory<byte>.Empty,
                    Type: FrameType.Error,
                    FrameError: new FrameError(IncompleteFrame, FrameErrorType.Incomplete));
            case 127:
                payloadLength64 = BinaryPrimitives.ReadUInt64BigEndian(span.Slice(2, 8));
                payloadStart = 10;
                break;
        }

        // Guard against insane payload sizes
        if (payloadLength64 > int.MaxValue)
        {
            return new WebsocketFrame(
                ReadOnlyMemory<byte>.Empty,
                Type: FrameType.Error,
                FrameError: new FrameError(PayloadTooLarge, FrameErrorType.PayloadTooLarge));
        }

        var payloadLength = (int)payloadLength64;

        // Make sure we have full frame (including mask)
        var totalHeaderLen = payloadStart + (isMasked ? 4 : 0);
        if (length < totalHeaderLen + payloadLength)
        {
            return new WebsocketFrame(
                ReadOnlyMemory<byte>.Empty,
                Type: FrameType.Error,
                FrameError: new FrameError(IncompleteFrame, FrameErrorType.Incomplete));
        }

        // Mask key
        Span<byte> maskKey = isMasked
            ? span.Slice(payloadStart, 4)
            : Span<byte>.Empty;

        if (isMasked)
        {
            payloadStart += 4;
        }

        var payloadSpan = span.Slice(payloadStart, payloadLength);

        // Unmask in-place (we own the buffer)
        if (isMasked)
        {
            for (var i = 0; i < payloadSpan.Length; i++)
            {
                payloadSpan[i] ^= maskKey[i & 0b11]; // i % 4
            }
        }

        ReadOnlyMemory<byte> payloadMemory;

        if (frameType == FrameType.Close)
        {
            // Interpret close code + reason *after* unmasking
            if (payloadSpan.Length >= 2)
            {
                var closeCode = BinaryPrimitives.ReadUInt16BigEndian(payloadSpan.Slice(0, 2));
                var reason = payloadSpan.Length > 2
                    ? Encoding.UTF8.GetString(payloadSpan[2..])
                    : null;

                var msg = $"Close frame received. Code: {closeCode}, Reason: {reason ?? "None"}";
                payloadMemory = Encoding.UTF8.GetBytes(msg).AsMemory();
            }
            else
            {
                // RFC allows zero-length close payload
                payloadMemory = ReadOnlyMemory<byte>.Empty;
            }
        }
        else
        {
            // For all other frames just return the raw payload
            payloadMemory = buffer.Slice(payloadStart, payloadLength);
        }

        return new WebsocketFrame(
            payloadMemory,
            frameType,
            Fin: fin);
    }
}
