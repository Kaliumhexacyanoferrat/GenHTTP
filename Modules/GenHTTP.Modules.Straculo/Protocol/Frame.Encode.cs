using System.Buffers;
using System.Buffers.Binary;
using GenHTTP.Modules.Straculo.Utils;

namespace GenHTTP.Modules.Straculo.Protocol;

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
    
    public static IMemoryOwner<byte> Encode(ReadOnlyMemory<byte> payload, byte opcode = 0x01, bool fin = true)
    {
        const int maxSmallPayloadLength = 125;
        var payloadLength = payload.Length;

        // Validate opcode: 0x01 (text), 0x02 (binary) or control (>= 0x08)
        if (opcode != 0x01 && opcode != 0x02 && opcode < 0x08)
        {
            throw new ArgumentException(
                "Invalid opcode. Must be 0x01 (text), 0x02 (binary), or a control frame opcode (>= 0x08).",
                nameof(opcode));
        }

        // Control frames MUST NOT be fragmented (RFC 6455)
        if (opcode >= 0x08 && !fin)
        {
            throw new ArgumentException(
                "Control frames (opcode >= 0x08) must not be fragmented (FIN must be true).",
                nameof(fin));
        }

        var responseLength = payloadLength switch
        {
            <= maxSmallPayloadLength => 2 + payloadLength,  // 1-byte length field
            <= ushort.MaxValue        => 4 + payloadLength,  // 2-byte extended length field
            _                         => 10 + payloadLength  // 8-byte extended length field
        };

        var arrayPool = ArrayPool<byte>.Shared;
        var responseBuffer = arrayPool.Rent(responseLength);

        try
        {
            var span = responseBuffer.AsSpan(0, responseLength);

            // FIN bit + opcode
            var finBit = fin ? (byte)0x80 : (byte)0x00;
            span[0] = (byte)(finBit | opcode);

            // Payload length field
            switch (payloadLength)
            {
                case <= maxSmallPayloadLength:
                    span[1] = (byte)payloadLength;
                    break;

                case <= ushort.MaxValue:
                    span[1] = 126;
                    BinaryPrimitives.WriteUInt16BigEndian(span[2..4], (ushort)payloadLength);
                    break;

                default:
                    span[1] = 127;
                    BinaryPrimitives.WriteUInt64BigEndian(span[2..10], (ulong)payloadLength);
                    break;
            }

            // Copy payload at the end
            payload.Span.CopyTo(span.Slice(responseLength - payloadLength));

            return new PooledMemoryOwner(responseBuffer, responseLength, arrayPool);
        }
        catch
        {
            arrayPool.Return(responseBuffer);
            throw;
        }
    }
}