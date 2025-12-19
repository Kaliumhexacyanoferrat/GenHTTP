using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace GenHTTP.Modules.Websockets.Protocol;

public static partial class Frame
{
    private const string IncompleteFrame = "Incomplete frame";
    private const string InvalidOpCode = "Invalid OpCode";
    private const string InvalidControlFrame = "Invalid Control Frame";
    private const string InvalidControlFrameLength = "Invalid Control Frame Length";
    private const string PayloadTooLarge = "Payload is too large";
    private const string NonWritablePipeSegment = "Pipe segment is not writable (cannot unmask in-place)";

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

    internal static WebsocketFrame Decode(
        ref ReadOnlySequence<byte> sequence,
        int rxMaxBufferSize,
        out SequencePosition consumed,
        out SequencePosition examined)
    {
        var reader = new SequenceReader<byte>(sequence);

        consumed = sequence.Start;
        examined = sequence.End;

        WebsocketFrame Incomplete(ref SequencePosition c, ref SequencePosition e, ref ReadOnlySequence<byte> seq)
        {
            c = seq.Start;
            e = seq.End;
            return new WebsocketFrame(new FrameError(IncompleteFrame, FrameErrorType.Incomplete));
        }

        if (reader.Remaining < 2)
            return Incomplete(ref consumed, ref examined, ref sequence);

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
            _    => FrameType.None
        };

        if (frameType == FrameType.None)
        {
            consumed = reader.Position;
            examined = reader.Position;
            return new WebsocketFrame(new FrameError(InvalidOpCode, FrameErrorType.InvalidOpCode));
        }

        var isControlFrame = frameType is FrameType.Close or FrameType.Ping or FrameType.Pong;
        var isMasked = (b1 & 0x80) != 0;
        var payloadLen7 = (byte)(b1 & 0x7F);

        if (isControlFrame && !fin)
        {
            consumed = reader.Position;
            examined = reader.Position;
            return new WebsocketFrame(new FrameError(InvalidControlFrame, FrameErrorType.InvalidControlFrame));
        }

        if (isControlFrame && payloadLen7 >= 126)
        {
            consumed = reader.Position;
            examined = reader.Position;
            return new WebsocketFrame(new FrameError(InvalidControlFrameLength, FrameErrorType.InvalidControlFrameLength));
        }

        long payloadLen64 = payloadLen7;

        if (payloadLen7 == 126)
        {
            if (reader.Remaining < 2)
                return Incomplete(ref consumed, ref examined, ref sequence);

            reader.TryReadBigEndian(out short len16);
            payloadLen64 = (ushort)len16;
        }
        else if (payloadLen7 == 127)
        {
            if (reader.Remaining < 8)
                return Incomplete(ref consumed, ref examined, ref sequence);

            reader.TryReadBigEndian(out long len64);
            payloadLen64 = len64;
        }

        const int MaxFrameHeaderSize = 14;
        var maxAllowedPayload = rxMaxBufferSize - MaxFrameHeaderSize;

        if (payloadLen64 < 0 || payloadLen64 > maxAllowedPayload || payloadLen64 > int.MaxValue)
        {
            consumed = reader.Position;
            examined = reader.Position;
            return new WebsocketFrame(new FrameError(PayloadTooLarge, FrameErrorType.PayloadTooLarge));
        }

        var payloadLength = (int)payloadLen64;

        Span<byte> maskKey = stackalloc byte[4];
        if (isMasked)
        {
            if (reader.Remaining < 4 || !reader.TryCopyTo(maskKey))
                return Incomplete(ref consumed, ref examined, ref sequence);

            reader.Advance(4);
        }

        if (reader.Remaining < payloadLength)
            return Incomplete(ref consumed, ref examined, ref sequence);

        // Payload slice (zero-copy)
        var payloadStart = reader.Position;
        var payloadEnd = sequence.GetPosition(payloadLength, payloadStart);
        var payloadSeq = sequence.Slice(payloadStart, payloadEnd);

        // Unmask in-place
        if (isMasked && payloadLength != 0)
        {
            if (!TryUnmaskInPlace(payloadSeq, maskKey))
            {
                consumed = sequence.Start;
                examined = sequence.End;
                return new WebsocketFrame(new FrameError(NonWritablePipeSegment, FrameErrorType.UndefinedBehavior));
            }
        }

        // Advance reader past payload for consumed/examined
        reader.Advance(payloadLength);
        consumed = reader.Position;
        examined = reader.Position;

        // Close frame
        if (frameType == FrameType.Close)
        {
            if (payloadLength >= 2)
            {
                // Read close code
                Span<byte> codeBuf = stackalloc byte[2];
                payloadSeq.Slice(0, 2).CopyTo(codeBuf);
                var closeCode = BinaryPrimitives.ReadUInt16BigEndian(codeBuf);

                // Read reason (allocates string, only for Close)
                string? reason = null;
                if (payloadLength > 2)
                {
                    // Unfortunately Encoding.GetString doesn't accept ReadOnlySequence,
                    // so we must materialize reason bytes OR decode manually.
                    // We'll keep it simple: materialize only the reason part.
                    var reasonBytes = payloadSeq.Slice(2).ToArray(); // alloc only for Close
                    reason = Encoding.UTF8.GetString(reasonBytes);
                }

                var msg = $"Close frame received. Code: {closeCode}, Reason: {reason ?? "None"}";
                var msgBytes = Encoding.UTF8.GetBytes(msg); // alloc only for Close

                var msgSeq = new ReadOnlySequence<byte>(msgBytes);
                return new WebsocketFrame(ref msgSeq, frameType, fin: fin);
            }
            else
            {
                // empty close payload
                var empty = ReadOnlySequence<byte>.Empty;
                return new WebsocketFrame(ref empty, frameType, fin: fin);
            }
        }
        // -------------------------------------------------------

        // Normal frames: return raw payload slice (zero-copy)
        return new WebsocketFrame(ref payloadSeq, frameType, fin: fin);
    }

    /// <summary>
    /// Brute force modifying the PipeReader's buffer, unmask the bytes.
    /// </summary>
    private static bool TryUnmaskInPlace(in ReadOnlySequence<byte> payload, ReadOnlySpan<byte> maskKey)
    {
        var maskOffset = 0;

        foreach (var mem in payload)
        {
            if (mem.Length == 0) continue;

            // We need a writable backing store. Pipe segments are usually byte[].
            if (!MemoryMarshal.TryGetArray(mem, out ArraySegment<byte> seg) ||
                seg.Array is null)
            {
                return false;
            }

            var span = seg.Array.AsSpan(seg.Offset, seg.Count);

            for (var i = 0; i < span.Length; i++)
            {
                span[i] ^= maskKey[(maskOffset + i) & 3];
            }

            maskOffset = (maskOffset + span.Length) & 3;
        }

        return true;
    }
}
