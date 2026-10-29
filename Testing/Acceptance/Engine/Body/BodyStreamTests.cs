using System.IO.Pipelines;
using System.Text;

using GenHTTP.Engine.Shared.Types.Body;

namespace GenHTTP.Testing.Acceptance.Engine.Body;

[TestClass]
public sealed class BodyStreamTests
{

    [TestMethod]
    public void TestBasics()
    {
        var framing = new ContentLengthFraming();
        framing.Apply(new Pipe().Reader, 5);

        var stream = new BodyStream(framing);

        Assert.IsTrue(stream.CanRead);
        Assert.IsFalse(stream.CanWrite);
        Assert.IsFalse(stream.CanSeek);

        Assert.AreEqual(5, stream.Length);
        Assert.AreEqual(0, stream.Position);

        Assert.ThrowsExactly<NotSupportedException>(() => stream.Flush());
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
        Assert.ThrowsExactly<NotSupportedException>(() => stream.SetLength(0));
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Write([], 0, 0));
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Position = 0);
    }

    [TestMethod]
    public void TestChunkedLengthIsUnknown()
    {
        var framing = new ChunkedFraming();
        framing.Apply(new Pipe().Reader);

        var stream = new BodyStream(framing);

        Assert.ThrowsExactly<NotSupportedException>(() => _ = stream.Length);
    }

    [TestMethod]
    public async Task TestReadOverloads()
    {
        var pipe = new Pipe();

        await pipe.Writer.WriteAsync("HelloWorld!"u8.ToArray());
        await pipe.Writer.CompleteAsync();

        var framing = new ContentLengthFraming();
        framing.Apply(pipe.Reader, 11);

        var stream = new BodyStream(framing);

        var buffer = new byte[5];

        Assert.AreEqual(5, await stream.ReadAsync(buffer, 0, 5, CancellationToken.None));
        Assert.AreEqual("Hello", Encoding.ASCII.GetString(buffer));

        Assert.AreEqual(5, stream.Read(buffer, 0, 5));
        Assert.AreEqual("World", Encoding.ASCII.GetString(buffer));

        Assert.AreEqual(1, await stream.ReadAsync(buffer.AsMemory()));
        Assert.AreEqual(11, stream.Position);

        Assert.AreEqual(0, await stream.ReadAsync(buffer.AsMemory()));
    }

}
