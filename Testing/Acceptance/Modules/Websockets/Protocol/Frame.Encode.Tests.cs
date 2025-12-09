using System.Buffers.Binary;
using System.Text;
using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Protocol;

[TestClass]
public sealed class Frame_Encode_Tests
{
    /*
       Encode_TextFrame_SmallPayload_CorrectHeaderAndPayload
       Verifies that small text frames (opcode=0x01, FIN=1) produce:
           Correct first byte (0x81)
           Correct 1-byte payload length
           Correct, unchanged payload bytes
           Runs for 3 different small text payloads.

       Encode_BinaryFrame_SmallPayload_SetsBinaryOpcode
       Ensures a small binary frame (opcode=0x02) sets the correct first byte (0x82) and correctly embeds the payload.

       Encode_TextFrame_NonFinal_ClearsFinBit
       Confirms that when fin=false, the FIN bit is properly cleared (0x01) while encoding text frames.
       Runs for 3 different payloads.

       Encode_Extended126Payload_Writes16BitLength
       Validates that payloads exactly 126 bytes use the correct extended 16-bit length field:
           frame[1] == 126
           Correct big-endian 16-bit length
           Payload follows starting at offset 4.

       Encode_Extended127Payload_Writes64BitLength
       Confirms that very large payloads (>65535 bytes) use the 64-bit extended length field:
           frame[1] == 127
           Correct big-endian 64-bit payload length
           Payload bytes correctly copied (spot-checked).

       Encode_ControlFrame_WithFinFalse_Throws
       Ensures that control frames (opcode >= 0x08) throw an exception when encoded with fin=false, as required by RFC 6455.

       Encode_InvalidOpcodeBelowControlRange_Throws
       Validates that invalid opcodes (e.g., 0x03) correctly throw an exception indicating the opcode is invalid.

       Encode_ControlFrame_Close_SmallPayload_CorrectHeaderAndPayload
       Ensures that close frames (opcode=0x08) with a small payload:
           Use correct first byte (0x88)
           Encode the correct payload length
           Copy the payload bytes unchanged.
     */

    private static byte[] EncodeToArray(ReadOnlyMemory<byte> payload, byte opcode = 0x01, bool fin = true)
    {
        using var owner = Frame.Encode(payload, opcode, fin);

        var expectedLength = GetExpectedFrameLength(payload.Length);
        return owner.Memory[..expectedLength].ToArray();
    }

    private static int GetExpectedFrameLength(int payloadLength)
    {
        const int maxSmallPayloadLength = 125;

        return payloadLength switch
        {
            <= maxSmallPayloadLength => 2 + payloadLength, // 1-byte length
            <= ushort.MaxValue => 4 + payloadLength, // 2-byte length
            _ => 10 + payloadLength // 8-byte length
        };
    }

    [TestMethod]
    [DataRow("a")]
    [DataRow("chunk")]
    [DataRow("this is a longer text payload")]
    public void Encode_TextFrame_SmallPayload_CorrectHeaderAndPayload(string text)
    {
        var payload = Encoding.UTF8.GetBytes(text);

        var frame = EncodeToArray(payload);

        // FIN=1, opcode=0x01
        Assert.AreEqual(0x81, frame[0]);
        // No MASK bit, length = 2
        Assert.AreEqual((byte)payload.Length, frame[1]);

        var actualPayload = new byte[payload.Length];
        Array.Copy(frame, 2, actualPayload, 0, payload.Length);

        CollectionAssert.AreEqual(payload, actualPayload);
    }

    [TestMethod]
    public void Encode_BinaryFrame_SmallPayload_SetsBinaryOpcode()
    {
        var payload = new byte[]
        {
            0x01, 0x02, 0x03
        };

        var frame = EncodeToArray(payload, opcode: 0x02);

        // FIN=1, opcode=0x02
        Assert.AreEqual(0x82, frame[0]);
        // Length = 3
        Assert.AreEqual((byte)payload.Length, frame[1]);

        var actualPayload = new byte[payload.Length];
        Array.Copy(frame, 2, actualPayload, 0, payload.Length);

        CollectionAssert.AreEqual(payload, actualPayload);
    }

    [TestMethod]
    [DataRow("a")]
    [DataRow("chunk")]
    [DataRow("this is a longer text payload")]
    public void Encode_TextFrame_NonFinal_ClearsFinBit(string text)
    {
        var payload = Encoding.UTF8.GetBytes(text);

        var frame = EncodeToArray(payload, opcode: 0x01, fin: false);

        // FIN=0, opcode=0x01 => 0x01
        Assert.AreEqual(0x01, frame[0] & 0x8F); // mask off RSV bits just in case
        Assert.AreEqual((byte)payload.Length, frame[1]);
    }

    [TestMethod]
    public void Encode_Extended126Payload_Writes16BitLength()
    {
        // Payload length just over small threshold (e.g., 126)
        var payload = new byte[126];
        for (var i = 0; i < payload.Length; i++)
        {
            payload[i] = (byte)'a';
        }

        var frame = EncodeToArray(payload);

        // FIN=1, opcode=0x01
        Assert.AreEqual(0x81, frame[0]);
        // PayloadLen7 = 126
        Assert.AreEqual(126, frame[1]);

        var len16 = BinaryPrimitives.ReadUInt16BigEndian(frame.AsSpan(2, 2));
        Assert.AreEqual((ushort)payload.Length, len16);

        // Payload starts at index 4
        for (var i = 0; i < payload.Length; i++)
        {
            Assert.AreEqual((byte)'a', frame[4 + i]);
        }
    }

    [TestMethod]
    public void Encode_Extended127Payload_Writes64BitLength()
    {
        // Payload length > ushort.MaxValue to force 64-bit length field
        const int payloadLength = ushort.MaxValue + 1; // 65536
        var payload = new byte[payloadLength];
        for (var i = 0; i < payload.Length; i++)
        {
            payload[i] = (byte)(i & 0xFF);
        }

        var frame = EncodeToArray(payload);

        // FIN=1, opcode=0x01
        Assert.AreEqual(0x81, frame[0]);
        // PayloadLen7 = 127
        Assert.AreEqual(127, frame[1]);

        var len64 = BinaryPrimitives.ReadUInt64BigEndian(frame.AsSpan(2, 8));
        Assert.AreEqual((ulong)payloadLength, len64);

        // Spot-check a few payload bytes
        Assert.AreEqual(payload[0], frame[10]);
        Assert.AreEqual(payload[12345], frame[10 + 12345]);
        Assert.AreEqual(payload[^1], frame[^1]);
    }

    [TestMethod]
    public void Encode_ControlFrame_WithFinFalse_Throws()
    {
        var payload = ReadOnlyMemory<byte>.Empty;

        var ex = Assert.Throws<ArgumentException>(() => Frame.Encode(payload, opcode: 0x08, fin: false));

        Assert.AreEqual("fin", ex.ParamName);
        Assert.Contains("Control frames", ex.Message);
    }

    [TestMethod]
    public void Encode_InvalidOpcodeBelowControlRange_Throws()
    {
        var payload = ReadOnlyMemory<byte>.Empty;

        var ex = Assert.Throws<ArgumentException>(() => Frame.Encode(payload, opcode: 0x03, fin: true));

        Assert.AreEqual("opcode", ex.ParamName);
        Assert.Contains("Invalid opcode", ex.Message);
    }

    [TestMethod]
    public void Encode_ControlFrame_Close_SmallPayload_CorrectHeaderAndPayload()
    {
        // Close frame with small payload
        var payload = new byte[]
        {
            0x03, 0xE8
        }; // code 1000

        var frame = EncodeToArray(payload, opcode: 0x08, fin: true);

        // FIN=1, opcode=0x08
        Assert.AreEqual(0x88, frame[0]);
        // Length = 2
        Assert.AreEqual((byte)payload.Length, frame[1]);

        var actualPayload = new byte[payload.Length];
        Array.Copy(frame, 2, actualPayload, 0, payload.Length);

        CollectionAssert.AreEqual(payload, actualPayload);
    }
}
