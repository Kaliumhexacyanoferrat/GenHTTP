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

        Assert.ThrowsExactly<NotSupportedException>(() => _ = stream.Position);
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Position = 0);
    }

    [TestMethod]
    public async Task TestReadAsyncByteArrayOverload()
    {
        var stream = await CreateAsync("5\r\nHello\r\n0\r\n\r\n");

        var buffer = new byte[5];
        var read = await stream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None);

        Assert.AreEqual(5, read);
        Assert.AreEqual("Hello", Encoding.ASCII.GetString(buffer));
    }

    [TestMethod]
    public async Task TestReadIntoEmptyBufferReturnsZero()
    {
        var stream = await CreateAsync("5\r\nHello\r\n0\r\n\r\n");

        Assert.AreEqual(0, await stream.ReadAsync(Memory<byte>.Empty));
    }

    [TestMethod]
    public async Task TestReadOnEmptyCompletedPipeReturnsZero()
    {
        var stream = await CreateAsync("");

        Assert.AreEqual(0, await stream.ReadAsync(new byte[16]));
    }

    [TestMethod]
    public async Task TestTruncatedChunkThrows()
    {
        var stream = await CreateAsync("5\r\nHel");

#pragma warning disable CA2022 // expected to throw before any byte count is returned
        await Assert.ThrowsExactlyAsync<InvalidDataException>(async () => await stream.ReadAsync(new byte[16]));
#pragma warning restore CA2022
    }

    [TestMethod]
    public async Task TestDrainOnEmptyCompletedPipe()
    {
        var stream = await CreateAsync("");

        await stream.DrainAsync();
    }

    [TestMethod]
    public async Task TestDrainOnTruncatedChunkReturnsWithoutThrowing()
    {
        var stream = await CreateAsync("5\r\nHel");

        await stream.DrainAsync();
    }

    private static async Task<ChunkedBodyStream> CreateAsync(string chunkedBody)
    {
        var pipe = new Pipe();

        await pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes(chunkedBody));
        await pipe.Writer.CompleteAsync();

        return new ChunkedBodyStream(pipe.Reader);
    }

}
