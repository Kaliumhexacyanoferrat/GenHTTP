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

        Assert.ThrowsExactly<NotSupportedException>(() => _ = stream.Position);
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Position = 0);

        Assert.ThrowsExactly<NotSupportedException>(() => stream.Read(Span<byte>.Empty));

        stream.Flush();
    }

    [TestMethod]
    public async Task TestReadAsyncNotSupported()
    {
        using var stream = new WriterStreamAdapter(new ArrayBufferWriter<byte>());

#pragma warning disable CA2022 // both calls are expected to throw before any byte count is returned
        await Assert.ThrowsExactlyAsync<NotSupportedException>(async () => await stream.ReadAsync(Memory<byte>.Empty));
        await Assert.ThrowsExactlyAsync<NotSupportedException>(async () => await stream.ReadAsync([], 0, 1, CancellationToken.None));
#pragma warning restore CA2022
    }

    [TestMethod]
    public async Task TestWriteAsyncByteArrayOverload()
    {
        var writer = new ArrayBufferWriter<byte>();

        using var stream = new WriterStreamAdapter(writer);

        await stream.WriteAsync("Hi"u8.ToArray(), 0, 2, CancellationToken.None);

        Assert.AreEqual("Hi", Encoding.ASCII.GetString(writer.WrittenSpan));

        await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await stream.WriteAsync([], 0, 0, new CancellationToken(true)));
    }

    [TestMethod]
    public async Task TestFlushAsyncCancelled()
    {
        using var stream = new WriterStreamAdapter(new ArrayBufferWriter<byte>());

        await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await stream.FlushAsync(new CancellationToken(true)));
    }

}
