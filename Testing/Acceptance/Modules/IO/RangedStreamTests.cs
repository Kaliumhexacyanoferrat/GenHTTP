using System.Text;
using GenHTTP.Modules.IO.Ranges;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public class RangedStreamTests
{

    [TestMethod]
    public void TestFullRange()
    {
        Assert.AreEqual("0123456789", GetRange(0, 9, 0, 10));
    }

    [TestMethod]
    public void TestNotAllWritten()
    {
        Assert.AreEqual("23", GetRange(0, 9, 2, 2));
    }

    [TestMethod]
    public void TestRangeExtracted()
    {
        Assert.AreEqual("3456", GetRange(3, 6, 0, 10));
    }

    [TestMethod]
    public void TestNothingToWrite()
    {
        Assert.AreEqual("", GetRange(12, 14, 0, 10));
    }

    [TestMethod]
    public void TestEndOfLargeFile()
    {
        Assert.AreEqual("12345", GetRange(10_001, 10_005, 0, 10, 10_000));
    }

    [TestMethod]
    public void TestSomewhereInLargeFile()
    {
        Assert.AreEqual("0123456789", GetRange(0, 10_000, 0, 10, 5000));
    }

    [TestMethod]
    public void TestOutOfLargeFile()
    {
        Assert.AreEqual("", GetRange(0, 10_000, 0, 10, 15_000));
    }

    [TestMethod]
    public void TestEndClampsToPositionNotStart()
    {
        Assert.AreEqual("01", GetRange(2, 5, 0, 10, 4));
    }

    [TestMethod]
    public void TestMultipleBuffersDoNotExceedRange()
    {
        using var target = new MemoryStream();

        const int bufferSize = 8;
        const ulong start = 6;
        const ulong end = 17; // inclusive -> 12 bytes expected

        using var stream = new RangedStream(target, start, end);

        var source = new byte[32];
        for (var i = 0; i < source.Length; i++)
        {
            source[i] = (byte)i;
        }

        for (var offset = 0; offset < source.Length; offset += bufferSize)
        {
            stream.Write(source, offset, bufferSize);
        }

        var written = target.ToArray();

        Assert.AreEqual((int)(end - start + 1), written.Length);

        for (var i = 0; i < written.Length; i++)
        {
            Assert.AreEqual((byte)(start + (ulong)i), written[i]);
        }
    }

    [TestMethod]
    public void TestBasics()
    {
        using var stream = new RangedStream(new MemoryStream(), 0, 10);

        Assert.AreEqual(0, stream.Position);
        Assert.AreEqual(10, stream.Length);

        Assert.IsTrue(stream.CanWrite);

        Assert.IsFalse(stream.CanRead);
        Assert.IsFalse(stream.CanSeek);

        Assert.ThrowsExactly<NotSupportedException>(() => stream.Read([], 0, 1));

        Assert.ThrowsExactly<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));

        Assert.ThrowsExactly<NotSupportedException>(() => stream.SetLength(0));
    }

    private static string GetRange(ulong start, ulong end, int offset, int count, int position = 0)
    {
        using var target = new MemoryStream();

        using var stream = new RangedStream(target, start, end);

        stream.Position = position;

        stream.Write("0123456789"u8.ToArray(), offset, count);

        return Encoding.ASCII.GetString(target.ToArray());
    }
}
