using System.Buffers;
using System.Buffers.Binary;
using GenHTTP.Modules.Straculo.Utils;

namespace GenHTTP.Modules.Straculo.Protocol;

public partial class Frame
{
    public static IMemoryOwner<byte> Build(ReadOnlyMemory<byte> payload, byte opcode = 0x01)
    {
        // The maximum payload size that can be encoded in a single byte (7 bits) is 125 bytes.
        const int maxSmallPayloadLength = 125;
        // Get the actual length of the payload provided.
        var payloadLength = payload.Length;

        // Validate opcode
        if (opcode != 0x01 && opcode != 0x02 && opcode < 0x08)
        {
            throw new ArgumentException("Invalid opcode. Must be 0x01 (text), 0x02 (binary), or a control frame opcode.", nameof(opcode));
        }

        // Calculate the total response frame size based on the payload length.
        // WebSocket frames have the following format:
        //  - 2 bytes for small payloads (1-byte length field)
        //  - 4 bytes for extended payloads (2-byte length field for lengths 126-65535)
        //  - 10 bytes for large payloads (8-byte length field for lengths > 65535)
        var responseLength = payloadLength switch
        {
            <= maxSmallPayloadLength => 2 + payloadLength,  // 1-byte length field
            <= ushort.MaxValue => 4 + payloadLength,        // 2-byte extended length field
            _ => 10 + payloadLength                         // 8-byte extended length field
        };

        // Rent the buffer from ArrayPool
        var arrayPool = ArrayPool<byte>.Shared;
        var responseBuffer = arrayPool.Rent(responseLength);

        try
        {
            // Get a span for efficient manipulation of the memory buffer.
            var span = responseBuffer.AsSpan(0, responseLength);

            // Set the first byte of the frame:
            // - FIN bit set (0x80) to indicate this is the final fragment of a message.
            // - Opcode set to 0x01 for a text frame.
            // Set FIN bit (final fragment) and opcode
            span[0] = (byte)(0x80 | opcode);

            // Write payload length
            switch (payloadLength)
            {
                case <= maxSmallPayloadLength:
                    // If the payload length is 125 bytes or less, encode it directly in the second byte.
                    span[1] = (byte)payloadLength;
                    break;
                case <= ushort.MaxValue:
                    // For payload length between 126 and 65535
                    span[1] = 126;
                    BinaryPrimitives.WriteUInt16BigEndian(span[2..4], (ushort)payloadLength);
                    break;
                default:
                    // For payload length exceeding 65535
                    span[1] = 127;
                    BinaryPrimitives.WriteUInt64BigEndian(span[2..10], (ulong)payloadLength);
                    break;
            }

            // Copy payload into the buffer
            payload.Span.CopyTo(span.Slice(responseLength - payloadLength));

            // Return the buffer wrapped in a memory owner
            return new PooledMemoryOwner(responseBuffer, responseLength, arrayPool);
        }
        catch
        {
            // Return the buffer to the pool in case of an exception
            arrayPool.Return(responseBuffer);
            throw;
        }
    }
}