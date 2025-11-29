using System.Buffers.Binary;
using GenHTTP.Modules.Straculo.Protocol;

namespace GenHTTP.Testing.Acceptance.Modules.Straculo.Protocol;

[TestClass]
public class Frame_Decode_Tests
{
    /*
       Decode_IncompleteHeader_ReturnsErrorIncomplete
       Ensures that frames with fewer than 2 bytes return an Incomplete error.
       
       Decode_InvalidOpcode_ReturnsInvalidOpCodeError
       Checks that reserved/unknown opcodes produce an InvalidOpCode error.
       
       Decode_ControlFrameFragmented_ReturnsInvalidControlFrameError
       Validates that control frames with FIN = 0 (fragmented) return an error.
       
       Decode_ControlFrameTooLong_ReturnsInvalidControlFrameLengthError
       Ensures control frames with payload lengths ≥ 126 are rejected.
       
       Decode_MaskedTextFrame_UnmasksPayloadCorrectly
       Verifies that masked text frames are properly unmasked to their original payload.
       
       Decode_CloseFrame_WithCodeAndReason_FormatsMessage
       Confirms that Close frames decode the close code and reason, producing the formatted message.
       
       Decode_CloseFrame_WithoutPayload_AllowsEmpty
       Ensures Close frames with zero-length payloads decode successfully.
       
       Decode_Extended126_UnmaskedTextFrame_Works
       Tests decoding of unmasked text frames using 16-bit extended payload lengths.
       
       Decode_Extended127_PayloadTooLarge_ReturnsError
       Ensures payload sizes larger than int.MaxValue return a PayloadTooLarge error.
       
       Decode_IncompleteExtendedFrame_ReturnsIncompleteError
       Checks that frames indicating extended lengths but missing required bytes return an Incomplete error.
     */
    
    private static WebsocketFrame Decode(Memory<byte> frame) => Frame.Decode(frame, frame.Length);

    [TestMethod]
    public void Decode_IncompleteHeader_ReturnsErrorIncomplete()
    {
        // Only 1 byte -> < 2
        var frame = new byte[] { 0x81 };

        var result = Decode(frame);

        Assert.AreEqual(FrameType.Error, result.Type);
        Assert.IsNotNull(result.FrameError);
        Assert.AreEqual(FrameErrorType.Incomplete, result.FrameError!.ErrorType);
    }
    
    [TestMethod]
    public void Decode_InvalidOpcode_ReturnsInvalidOpCodeError()
    {
        // FIN=1, opcode=0x03 (reserved)
        var frame = new byte[]
        {
            0x83, // 1000 0011 => FIN + opcode 3
            0x00  // no mask, length 0
        };

        var result = Decode(frame);

        Assert.AreEqual(FrameType.Error, result.Type);
        Assert.IsNotNull(result.FrameError);
        Assert.AreEqual(FrameErrorType.InvalidOpCode, result.FrameError!.ErrorType);
    }
    
    [TestMethod]
    public void Decode_ControlFrameFragmented_ReturnsInvalidControlFrameError()
    {
        // FIN=0, opcode=0x09 (Ping, control frame) -> invalid (control MUST NOT be fragmented)
        var frame = new byte[]
        {
            0x09, // 0000 1001 => no FIN, opcode Ping
            0x00  // no mask, length 0
        };

        var result = Decode(frame);

        Assert.AreEqual(FrameType.Error, result.Type);
        Assert.IsNotNull(result.FrameError);
        Assert.AreEqual(FrameErrorType.InvalidControlFrame, result.FrameError!.ErrorType);
    }
    
    [TestMethod]
    public void Decode_ControlFrameTooLong_ReturnsInvalidControlFrameLengthError()
    {
        // Ping with payloadLen7 = 126 (0x7E) -> illegal for control frames
        var frame = new byte[]
        {
            0x89, // FIN=1, opcode=Ping
            0x7E  // MASK=0, payloadLen7=126
        };

        var result = Decode(frame);

        Assert.AreEqual(FrameType.Error, result.Type);
        Assert.IsNotNull(result.FrameError);
        Assert.AreEqual(FrameErrorType.InvalidControlFrameLength, result.FrameError!.ErrorType);
    }
    
    [TestMethod]
    public void Decode_MaskedTextFrame_UnmasksPayloadCorrectly()
    {
        // Build a masked text frame "Hi"
        var payload = "Hi"u8.ToArray();
        var maskKey = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        var maskedPayload = new byte[payload.Length];
        for (int i = 0; i < payload.Length; i++)
        {
            maskedPayload[i] = (byte)(payload[i] ^ maskKey[i & 0x03]);
        }

        var frame = new byte[2 + 4 + maskedPayload.Length];
        frame[0] = 0x81; // FIN=1, Text
        frame[1] = (byte)(0x80 | payload.Length); // MASK bit + length
        Array.Copy(maskKey, 0, frame, 2, 4);
        Array.Copy(maskedPayload, 0, frame, 6, maskedPayload.Length);

        var result = Decode(frame);

        Assert.AreEqual(FrameType.Text, result.Type);
        Assert.IsTrue(result.Fin);
        
        Assert.AreEqual("Hi", result.DataAsString);
    }
    
    [TestMethod]
    public void Decode_CloseFrame_WithCodeAndReason_FormatsMessage()
    {
        // Close frame, FIN=1, opcode=0x08, masked
        // Payload: [1000 big endian][UTF8("Bye")]
        ushort closeCode = 1000;
        var reasonBytes = "Bye"u8.ToArray();
        var payload = new byte[2 + reasonBytes.Length];
        BinaryPrimitives.WriteUInt16BigEndian(payload, closeCode);
        Array.Copy(reasonBytes, 0, payload, 2, reasonBytes.Length);

        var maskKey = new byte[] { 0x10, 0x20, 0x30, 0x40 };

        var maskedPayload = new byte[payload.Length];
        for (int i = 0; i < payload.Length; i++)
        {
            maskedPayload[i] = (byte)(payload[i] ^ maskKey[i & 0x03]);
        }

        var frame = new byte[2 + 4 + maskedPayload.Length];
        frame[0] = 0x88; // FIN=1, Close
        frame[1] = (byte)(0x80 | payload.Length); // MASK bit + length
        Array.Copy(maskKey, 0, frame, 2, 4);
        Array.Copy(maskedPayload, 0, frame, 6, maskedPayload.Length);

        var result = Decode(frame);

        Assert.AreEqual(FrameType.Close, result.Type);
        
        Assert.AreEqual("Close frame received. Code: 1000, Reason: Bye", result.DataAsString);
    }
    
    [TestMethod]
    public void Decode_CloseFrame_WithoutPayload_AllowsEmpty()
    {
        // Close frame with no payload
        var frame = new byte[]
        {
            0x88, // FIN=1, Close
            0x00  // no mask, length 0
        };

        var result = Decode(frame);

        Assert.AreEqual(FrameType.Close, result.Type);
        Assert.AreEqual(0, result.Data.Length);
    }
    
    [TestMethod]
    public void Decode_Extended126_UnmaskedTextFrame_Works()
    {
        // Text frame with payload length = 126 (extended 16-bit)
        var payload = new byte[126];
        for (int i = 0; i < payload.Length; i++)
        {
            payload[i] = (byte)'a';
        }

        var frame = new byte[2 + 2 + payload.Length];
        frame[0] = 0x81; // FIN=1, Text
        frame[1] = 0x7E; // MASK=0, payloadLen7=126
        // Extended length (16-bit BE)
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(2, 2), (ushort)payload.Length);
        Array.Copy(payload, 0, frame, 4, payload.Length);

        var result = Decode(frame);

        Assert.AreEqual(FrameType.Text, result.Type);
        Assert.AreEqual(126, result.Data.Length);
        foreach (var b in result.Data.Span)
        {
            Assert.AreEqual((byte)'a', b);
        }
    }
    
    [TestMethod]
    public void Decode_Extended127_PayloadTooLarge_ReturnsError()
    {
        // Text frame with extended 64-bit length > int.MaxValue
        var frame = new byte[10];
        frame[0] = 0x81; // FIN=1, Text
        frame[1] = 0x7F; // MASK=0, payloadLen7=127

        // 64-bit payload length = 2^31 (0x0000000080000000) > int.MaxValue
        var lengthBytes = new byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(lengthBytes, 0x0000000080000000UL);
        Array.Copy(lengthBytes, 0, frame, 2, 8);

        var result = Decode(frame);

        Assert.AreEqual(FrameType.Error, result.Type);
        Assert.IsNotNull(result.FrameError);
        Assert.AreEqual(FrameErrorType.PayloadTooLarge, result.FrameError!.ErrorType);
    }
    
    [TestMethod]
    public void Decode_IncompleteExtendedFrame_ReturnsIncompleteError()
    {
        // Indicate extended length (126) but not enough bytes for the extended length field.
        var frame = new byte[]
        {
            0x81, // FIN=1, Text
            0x7E  // MASK=0, payloadLen7=126 => expects 2 more bytes for length but they're missing
        };

        var result = Decode(frame);

        Assert.AreEqual(FrameType.Error, result.Type);
        Assert.IsNotNull(result.FrameError);
        Assert.AreEqual(FrameErrorType.Incomplete, result.FrameError!.ErrorType);
    }
}