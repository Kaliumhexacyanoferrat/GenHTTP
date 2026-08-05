using System.IO.Pipelines;
using System.Text;

using GenHTTP.Engine.Shared.Types.Body;

namespace GenHTTP.Testing.Acceptance.Engine.Body;

[TestClass]
public sealed class LengthLimitedStreamTests
{

    [TestMethod]
    public async Task TestReadLimitsToContentLength()
    {
        var pipe = new Pipe();

        await pipe.Writer.WriteAsync("Hello, World!"u8.ToArray());
        await pipe.Writer.CompleteAsync();

        var stream = new LengthLimitedStream(pipe.Reader, contentLength: 5);

        var buffer = new byte[10];
        var read = await stream.ReadAsync(buffer);

        Assert.AreEqual(5, read);
        Assert.AreEqual("Hello", Encoding.ASCII.GetString(buffer, 0, read));
        Assert.AreEqual(5, stream.Position);

        Assert.AreEqual(0, await stream.ReadAsync(buffer));

        await stream.DrainAsync(); // already exhausted, should just no-op
    }

    [TestMethod]
    public void TestBasics()
    {
        var stream = new LengthLimitedStream(new Pipe().Reader, contentLength: 5);

        Assert.IsTrue(stream.CanRead);
        Assert.IsFalse(stream.CanWrite);
        Assert.IsFalse(stream.CanSeek);
        Assert.AreEqual(5, stream.Length);

        Assert.ThrowsExactly<NotSupportedException>(() => stream.Flush());
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
        Assert.ThrowsExactly<NotSupportedException>(() => stream.SetLength(0));
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Write([], 0, 0));
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Position = 0);
    }

}
