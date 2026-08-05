using System.Buffers;
using System.Text;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public sealed class WriterStreamAdapterTests
{

    [TestMethod]
    public async Task TestWrite()
    {
        var writer = new ArrayBufferWriter<byte>();

        using var stream = new WriterStreamAdapter(writer);

        stream.WriteByte((byte)'H');
        stream.Write("i"u8.ToArray(), 0, 1);
        await stream.WriteAsync("!"u8.ToArray());
        await stream.FlushAsync();

        Assert.AreEqual("Hi!", Encoding.ASCII.GetString(writer.WrittenSpan));
    }

    [TestMethod]
    public void TestBasics()
    {
        using var stream = new WriterStreamAdapter(new ArrayBufferWriter<byte>());

        Assert.IsTrue(stream.CanWrite);
        Assert.IsFalse(stream.CanRead);
        Assert.IsFalse(stream.CanSeek);

        Assert.ThrowsExactly<NotSupportedException>(() => stream.Read([], 0, 1));
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
        Assert.ThrowsExactly<NotSupportedException>(() => stream.SetLength(0));
        Assert.ThrowsExactly<NotSupportedException>(() => _ = stream.Length);
    }

}
