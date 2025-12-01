using System.Buffers.Binary;
using System.Text;
using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Protocol;

[TestClass]
public sealed class Frame_Encode_CloseAsync_Tests
{
    /*

       EncodeClose_NoReason_BuildsValidCloseFrame
       Verifies that a close frame with no reason uses the correct header, payload length, and encodes the status code properly.

       EncodeClose_WithReason_BuildsValidCloseFrame
       Ensures that when a reason is included, the payload length is correct and the UTF-8 reason bytes and status code are encoded properly.

       EncodeClose_TooLongReason_ThrowsArgumentException
       Confirms that providing a reason longer than the allowed 125-byte control-frame limit results in an exception.

       EncodeClose_UsesFinBitAndCloseOpcode
       Checks that the FIN flag is set, the opcode is 0x08 (Close), and the overall frame length is correct.

     */

    private static byte[] EncodeCloseToArray(string? reason, ushort statusCode)
    {
        // Call your EncodeClose method and trim to the actual frame length
        using var owner = Frame.EncodeClose(reason, statusCode);

        var payloadLength = 2 + (reason is null ? 0 : Encoding.UTF8.GetByteCount(reason));
        // For close frames we always stay <= 125, so header is always 2 bytes
        var frameLength = 2 + payloadLength;

        return owner.Memory[..frameLength].ToArray();
    }

    [TestMethod]
    public void EncodeClose_NoReason_BuildsValidCloseFrame()
    {
        const ushort statusCode = 1000; // Normal closure
        const int payloadLength = 2; // Only status code
        const int frameLength = 2 + payloadLength;

        var frame = EncodeCloseToArray(reason: null, statusCode);

        // Assert: header
        Assert.AreEqual(0x88, frame[0], "FIN + opcode for Close must be 0x88 (FIN=1, opcode=0x8).");
        Assert.AreEqual((byte)payloadLength, frame[1], "Payload length must equal 2 (status code only).");
        Assert.HasCount(frameLength, frame, "Frame length must be 4 bytes.");

        // Assert: payload (status code, big-endian)
        var code = BinaryPrimitives.ReadUInt16BigEndian(frame.AsSpan(2, 2));
        Assert.AreEqual(statusCode, code, "Status code must be written in big-endian.");
    }

    [TestMethod]
    public void EncodeClose_WithReason_BuildsValidCloseFrame()
    {
        const ushort statusCode = 1001;
        const string reason = "Going away";
        var reasonBytes = Encoding.UTF8.GetBytes(reason);
        var payloadLength = 2 + reasonBytes.Length;
        var frameLength = 2 + payloadLength;

        var frame = EncodeCloseToArray(reason, statusCode);

        // Assert: header
        Assert.AreEqual(0x88, frame[0], "FIN + opcode for Close must be 0x88 (FIN=1, opcode=0x8).");
        Assert.AreEqual((byte)payloadLength, frame[1], "Payload length must match status code + reason.");
        Assert.HasCount(frameLength, frame, "Frame length must be header + payload.");

        // Assert: status code
        var code = BinaryPrimitives.ReadUInt16BigEndian(frame.AsSpan(2, 2));
        Assert.AreEqual(statusCode, code, "Status code must be written in big-endian.");

        // Assert: reason string
        var actualReasonBytes = frame.AsSpan(4, reasonBytes.Length);
        CollectionAssert.AreEqual(reasonBytes, actualReasonBytes.ToArray(), "Reason bytes must match UTF-8 encoding of the reason string.");
    }

    [TestMethod]
    public void EncodeClose_TooLongReason_ThrowsArgumentException()
    {
        var tooLongReason = new string('a', 200);

        Assert.Throws<ArgumentException>(() =>
        {
            Frame.EncodeClose(tooLongReason, 1000);
        });
    }

    [TestMethod]
    public void EncodeClose_UsesFinBitAndCloseOpcode()
    {
        const ushort statusCode = 1000;
        const string reason = "Test";

        var reasonBytes = Encoding.UTF8.GetBytes(reason);
        var payloadLength = 2 + reasonBytes.Length;
        var frameLength = 2 + payloadLength;

        var frame = EncodeCloseToArray(reason, statusCode);

        var finAndOpcode = frame[0];

        // FIN must be set
        Assert.AreNotEqual(0, finAndOpcode & 0x80, "FIN bit must be set for Close frames.");

        // Opcode must be 0x8
        var opcode = finAndOpcode & 0x0F;
        Assert.AreEqual(0x08, opcode, "Opcode must be 0x8 for Close frames.");

        // Sanity check on total length
        Assert.HasCount(frameLength, frame);
    }
}
