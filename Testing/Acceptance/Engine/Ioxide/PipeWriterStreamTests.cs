#if NET11_0_OR_GREATER

using System.Buffers;
using System.IO.Pipelines;
using System.Text;

using GenHTTP.Engine.Ioxide.Protocol.Http1;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

[TestClass]
public sealed class PipeWriterStreamTests
{

    [TestMethod]
    public void TestBasics()
    {
        var pipe = new Pipe();

        var stream = new PipeWriterStream(new ArrayBufferWriter<byte>(), pipe.Writer);

        Assert.IsFalse(stream.CanRead);
        Assert.IsFalse(stream.CanSeek);
        Assert.IsTrue(stream.CanWrite);

        Assert.ThrowsExactly<NotSupportedException>(() => _ = stream.Length);
        Assert.ThrowsExactly<NotSupportedException>(() => _ = stream.Position);
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Position = 0);

        Assert.ThrowsExactly<NotSupportedException>(() => stream.Read([], 0, 1));
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
        Assert.ThrowsExactly<NotSupportedException>(() => stream.SetLength(0));

        stream.Flush();
    }

    [TestMethod]
    public void TestWriteByte()
    {
        var writer = new ArrayBufferWriter<byte>();

        var stream = new PipeWriterStream(writer, new Pipe().Writer);

        stream.WriteByte((byte)'H');
        stream.WriteByte((byte)'i');

        Assert.AreEqual("Hi", Encoding.ASCII.GetString(writer.WrittenSpan));
    }

    [TestMethod]
    public async Task TestFlushAsyncDrainsUnderlyingPipe()
    {
        var pipe = new Pipe();

        var stream = new PipeWriterStream(pipe.Writer, pipe.Writer);

        await stream.WriteAsync("Hello"u8.ToArray());

        await stream.FlushAsync();

        var read = await pipe.Reader.ReadAsync();

        Assert.AreEqual("Hello", Encoding.ASCII.GetString(read.Buffer.ToArray()));
    }

}

#endif
