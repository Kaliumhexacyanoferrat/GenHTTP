using System.Buffers.Binary;
using System.Text;

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
            0x01 => FrameType.Text,
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
}