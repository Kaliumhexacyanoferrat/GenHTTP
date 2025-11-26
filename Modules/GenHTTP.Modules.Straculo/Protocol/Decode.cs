using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using GenHTTP.Modules.Straculo.Utils;

namespace GenHTTP.Modules.Straculo.Protocol;

public static partial class Frame
{
    public static ReadOnlyMemory<byte> Decode(Memory<byte> buffer, int length, out FrameType frameType)
    {
        // Get a span of the input buffer for efficient memory operations.
        var span = buffer.Span;

        // Validate that the buffer contains at least the minimum frame size (2 bytes).
        if (length < 2)
            throw new ArgumentException("Incomplete frame.", nameof(buffer));

        // Extract the opcode from the first byte.
        // The opcode determines the type of frame (e.g., text, binary, close).
        var opcode = span[0] & 0x0F;

        // Handle close frames separately.
        if (opcode == 0x08) // Close frame
        {
            frameType = FrameType.Close;

            // Extract the payload length from the second byte.
            var opPayloadLength = span[1] & 0x7F;

            // If the payload length is less than 2, return an empty payload.
            if (opPayloadLength < 2)
            {
                return ReadOnlyMemory<byte>.Empty;
            }

            // Extract the close code (2 bytes, big-endian).
            var closeCode = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(2, 2));

            // Extract the optional UTF-8 encoded reason phrase (if present).
            var reason = opPayloadLength > 2
                ? Encoding.UTF8.GetString(span.Slice(4, opPayloadLength - 2))
                : null;

            // Construct the close message and return it as a UTF-8 encoded memory block.
            var message = $"Close frame received. Code: {closeCode}, Reason: {reason ?? "None"}";
            return Encoding.UTF8.GetBytes(message).AsMemory();
        }

        // Determine the frame type based on the opcode.
        frameType = opcode switch
        {
            0x00 => FrameType.Continue,
            0x01 => FrameType.Utf8,
            0x02 => FrameType.Binary,
            0x09 => FrameType.Ping,
            0x0A => FrameType.Pong,
#pragma warning disable S3928
            _ => throw new ArgumentOutOfRangeException(nameof(opcode)) // Invalid opcode.
#pragma warning restore S3928
        };

        // Determine if the frame is masked (MASK bit is set in the second byte).
        var isMasked = (span[1] & 0x80) != 0;

        // Extract the base payload length (7 bits of the second byte).
        var payloadLength = span[1] & 0x7F;

        // Initialize the starting position of the payload based on the frame structure.
        var payloadStart = 2;

        // Handle extended payload lengths.
        switch (payloadLength)
        {
            case 126 when length < 4:
                // If the length is 126 but there are not enough bytes for the extended length field, throw an error.
                throw new ArgumentException("Incomplete frame.", nameof(buffer));
            case 126:
                // Extract the 16-bit extended payload length (big-endian) and adjust the payload start index.
                payloadLength = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(2, 2));
                payloadStart = 4;
                break;
            case 127 when length < 10:
                // If the length is 127 but there are not enough bytes for the extended length field, throw an error.
                throw new ArgumentException("Incomplete frame.", nameof(buffer));
            case 127:
                // Extract the 64-bit extended payload length (big-endian) and adjust the payload start index.
                payloadLength = (int)BinaryPrimitives.ReadUInt64BigEndian(span.Slice(2, 8));
                payloadStart = 10;
                break;
        }

        // Validate that the buffer contains enough bytes for the payload (including masking key if applicable).
        if (length < payloadStart + payloadLength + (isMasked ? 4 : 0))
            throw new ArgumentException("Incomplete frame.", nameof(buffer));

        // Extract the masking key if the frame is masked (4 bytes following the header).
        var maskKey = isMasked ? span.Slice(payloadStart, 4) : Span<byte>.Empty;
        payloadStart += isMasked ? 4 : 0;

        // Extract the payload data as a span for further processing.
        var payloadSpan = span.Slice(payloadStart, payloadLength);

        // If the frame is masked, unmask the payload using the XOR operation and the masking key.
        if (isMasked)
        {
            for (var i = 0; i < payloadSpan.Length; i++)
                payloadSpan[i] ^= maskKey[i % 4];
        }

        // Return the payload as raw bytes in a new memory block.
        return payloadSpan.ToArray();
    }
    
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
        // // WebSocket frames have the following format:
        // // - 2 bytes for small payloads (1-byte length field)
        // // - 4 bytes for extended payloads (2-byte length field for lengths 126-65535)
        // // - 10 bytes for large payloads (8-byte length field for lengths > 65535)
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