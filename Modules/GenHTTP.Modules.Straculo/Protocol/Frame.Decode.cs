using System.Buffers.Binary;
using System.Text;

namespace GenHTTP.Modules.Straculo.Protocol;

public static partial class Frame
{
    private const string IncompleteFrame = "Incomplete frame";
    
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
                FrameError: new FrameError(IncompleteFrame, FrameErrorType.InvalidOpCode));
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
                    FrameError: new FrameError(IncompleteFrame, FrameErrorType.InvalidControlFrame));
            // RFC 6455: control frames MUST have payload length <= 125
            case true when payloadLen7 >= 126:
                return new WebsocketFrame(
                    ReadOnlyMemory<byte>.Empty,
                    Type: FrameType.Error,
                    FrameError: new FrameError(IncompleteFrame, FrameErrorType.InvalidControlFrameLength));
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
                FrameError: new FrameError(IncompleteFrame, FrameErrorType.PayloadTooLarge));
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
            //payloadMemory = payloadSpan.ToArray();
            payloadMemory = buffer.Slice(payloadStart, payloadLength);
        }

        return new WebsocketFrame(
            payloadMemory,
            frameType,
            Fin: fin);
    }
}