using System.IO.Pipelines;
using System.Text;

using GenHTTP.Engine.Shared.Types.Body;

namespace GenHTTP.Testing.Acceptance.Engine.Body;

[TestClass]
public sealed class ChunkedBodyStreamTests
{

    [TestMethod]
    public async Task TestReadAcrossOverflow()
    {
        var pipe = new Pipe();

        await pipe.Writer.WriteAsync("5\r\nHello\r\n0\r\n\r\n"u8.ToArray());
        await pipe.Writer.CompleteAsync();

        var stream = new ChunkedBodyStream(pipe.Reader);

        var result = new List<byte>();
        var buffer = new byte[2]; // smaller than the chunk, forces the overflow path

        int read;

        while ((read = await stream.ReadAsync(buffer)) > 0)
        {
            result.AddRange(buffer[..read]);
        }

        Assert.AreEqual("Hello", Encoding.ASCII.GetString(result.ToArray()));

        await stream.DrainAsync(); // already completed, should just no-op
    }

    [TestMethod]
    public void TestBasics()
    {
        var stream = new ChunkedBodyStream(new Pipe().Reader);

        Assert.IsTrue(stream.CanRead);
        Assert.IsFalse(stream.CanWrite);
        Assert.IsFalse(stream.CanSeek);

        Assert.ThrowsExactly<NotSupportedException>(() => stream.Flush());
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
        Assert.ThrowsExactly<NotSupportedException>(() => stream.SetLength(0));
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Write([], 0, 0));
        Assert.ThrowsExactly<NotSupportedException>(() => _ = stream.Length);
    }

}
