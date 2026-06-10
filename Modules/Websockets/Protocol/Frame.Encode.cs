using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace GenHTTP.Modules.Websockets.Protocol;

public partial class Frame
{
    
    /* Websockets RFC 6455 Frame Encode definition (LLM generated)

      A WebSocket frame is built (or encoded) by constructing a small header followed by the raw payload bytes,
      producing a contiguous binary block that conforms to RFC 6455. The first header byte contains the FIN bit
      and the opcode, indicating whether the frame is the final fragment of a message and whether it
      represents text, binary, ping, pong, or close. The second header byte contains the MASK bit (set only for frames sent by a client)
      and a 7-bit payload length, which may either store the actual length (0–125) or a special marker (126 or 127)
      indicating that an extended 16-bit or 64-bit length field follows. Payload lengths of 126 require two extra bytes,
      and lengths of 127 require eight extra bytes. After the length fields, the optional 4-byte masking key appears for
      client-to-server frames; the payload must then be XOR-masked with this key. Once the header is complete, the raw payload
      bytes are appended to form the final encoded frame. The server typically sends unmasked frames, producing the
      sequence: header → extended length fields (if any) → payload. This produces a valid, self-contained WebSocket frame
      ready to be transmitted over the TCP stream.

     */

    /// <summary>
    /// Writes a WebSocket frame directly into <paramref name="writer"/>, borrowing its buffer memory
    /// for the header and copying the payload in once — no intermediate ArrayPool rental.
    /// </summary>
    public static void Write(IBufferWriter<byte> writer, ReadOnlyMemory<byte> payload, byte opcode = 0x01, bool fin = true)
        => Write(writer, payload.Span, opcode, fin);

    private static void Write(IBufferWriter<byte> writer, ReadOnlySpan<byte> payload, byte opcode, bool fin)
    {
        const int maxSmallPayloadLength = 125;
        
        var payloadLength = payload.Length;

        if (opcode != 0x01 && opcode != 0x02 && opcode < 0x08)
        {
            throw new ArgumentException("Invalid opcode. Must be 0x01 (text), 0x02 (binary), or a control frame opcode (>= 0x08).", nameof(opcode));
        }

        if (opcode >= 0x08 && !fin)
        {
            throw new ArgumentException("Control frames (opcode >= 0x08) must not be fragmented (FIN must be true).", nameof(fin));
        }

        var headerLength = payloadLength switch
        {
            <= maxSmallPayloadLength => 2,
            <= ushort.MaxValue       => 4,
            _                        => 10
        };

        var header = writer.GetSpan(headerLength);

        var finBit = fin ? (byte)0x80 : (byte)0x00;
        header[0] = (byte)(finBit | opcode);

        switch (payloadLength)
        {
            case <= maxSmallPayloadLength:
                header[1] = (byte)payloadLength;
                break;

            case <= ushort.MaxValue:
                header[1] = 126;
                BinaryPrimitives.WriteUInt16BigEndian(header[2..4], (ushort)payloadLength);
                break;

            default:
                header[1] = 127;
                BinaryPrimitives.WriteUInt64BigEndian(header[2..10], (ulong)payloadLength);
                break;
        }

        writer.Advance(headerLength);

        if (payloadLength > 0)
        {
            writer.Write(payload);
        }
    }

    public static void WritePing(IBufferWriter<byte> writer)
        => Write(writer, ReadOnlySpan<byte>.Empty, opcode: 0x09, fin: true);

    public static void WritePong(IBufferWriter<byte> writer, ReadOnlyMemory<byte> payload)
        => Write(writer, payload.Span, opcode: 0x0A, fin: true);

    public static void WriteClose(IBufferWriter<byte> writer, string? reason, ushort statusCode)
    {
        if (string.IsNullOrEmpty(reason))
        {
            Span<byte> status = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(status, statusCode);
            Write(writer, status, opcode: 0x08, fin: true);
            return;
        }

        var reasonBytes = Encoding.UTF8.GetBytes(reason);
        var payloadLength = 2 + reasonBytes.Length;

        if (payloadLength > 125)
        {
            throw new ArgumentException("Close reason too long (must fit in 125 bytes including the 2-byte status code).", nameof(reason));
        }

        Span<byte> payload = stackalloc byte[payloadLength];
        BinaryPrimitives.WriteUInt16BigEndian(payload[0..2], statusCode);
        reasonBytes.AsSpan().CopyTo(payload[2..]);

        Write(writer, payload, opcode: 0x08, fin: true);
    }
    
}