using System.IO.Pipelines;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types.Body;

using static GenHTTP.Testing.Acceptance.Engine.Body.ContentLengthFramingTests;

namespace GenHTTP.Testing.Acceptance.Engine.Body;

[TestClass]
public sealed class ChunkedFramingTests
{
    private const string Body = "5\r\nHello\r\n6\r\n World\r\n0\r\n\r\n";

    [TestMethod]
    public async Task TestReadWithSmallBuffer()
    {
        var (pipe, framing) = await CreateAsync(Body + "NEXT");

        var result = new StringBuilder();
        var buffer = new byte[2];

        int read;

        while ((read = await framing.ReadAsync(buffer, CancellationToken.None)) > 0)
        {
            result.Append(Encoding.ASCII.GetString(buffer, 0, read));
        }

        Assert.AreEqual("Hello World", result.ToString());
        Assert.AreEqual(11, framing.Position);
        Assert.IsNull(framing.Length);

        Assert.AreEqual(0, await framing.ReadAsync(buffer, CancellationToken.None));

        Assert.IsTrue(await framing.SkipAsync());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestChunkSpanningSegments()
    {
        var payload = new byte[20_000];
        new Random(42).NextBytes(payload);

        var pipe = new Pipe();

        await pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes($"{payload.Length:X}\r\n"));
        await pipe.Writer.WriteAsync(payload.AsMemory(0, 5000));
        await pipe.Writer.WriteAsync(payload.AsMemory(5000));
        await pipe.Writer.WriteAsync("\r\n0\r\n\r\nNEXT"u8.ToArray());
        await pipe.Writer.CompleteAsync();

        var framing = new ChunkedFraming();
        framing.Apply(pipe.Reader);

        var result = new List<byte>();
        var buffer = new byte[3000]; // does not divide the segment sizes, so reads cross segment boundaries

        int read;

        while ((read = await framing.ReadAsync(buffer, CancellationToken.None)) > 0)
        {
            result.AddRange(buffer[..read]);
        }

        CollectionAssert.AreEqual(payload, result.ToArray());

        Assert.IsTrue(await framing.SkipAsync());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestReadWholeChunks()
    {
        var payload = new byte[5000];
        new Random(13).NextBytes(payload);

        var pipe = new Pipe();

        await pipe.Writer.WriteAsync("5\r\nHello\r\n"u8.ToArray());
        await pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes($"{payload.Length:X}\r\n"));
        await pipe.Writer.WriteAsync(payload.AsMemory(0, 1000));
        await pipe.Writer.WriteAsync(payload.AsMemory(1000));
        await pipe.Writer.WriteAsync("\r\n0\r\n\r\nNEXT"u8.ToArray());
        await pipe.Writer.CompleteAsync();

        var framing = new ChunkedFraming();
        framing.Apply(pipe.Reader);

        var buffer = new byte[8192];

        Assert.AreEqual(5, await framing.ReadAsync(buffer, CancellationToken.None));
        Assert.AreEqual("Hello", Encoding.ASCII.GetString(buffer, 0, 5));

        Assert.AreEqual(payload.Length, await framing.ReadAsync(buffer, CancellationToken.None));
        CollectionAssert.AreEqual(payload, buffer[..payload.Length]);

        Assert.AreEqual(5 + payload.Length, framing.Position);

        Assert.AreEqual(0, await framing.ReadAsync(buffer, CancellationToken.None));

        Assert.IsTrue(await framing.SkipAsync());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestReadToEndOfChunkSpanningSegments()
    {
        var payload = new byte[20_000];
        new Random(7).NextBytes(payload);

        var pipe = new Pipe();

        await pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes($"{payload.Length:X}\r\n"));
        await pipe.Writer.WriteAsync(payload);
        await pipe.Writer.WriteAsync("\r\n0\r\n\r\n"u8.ToArray());
        await pipe.Writer.CompleteAsync();

        var framing = new ChunkedFraming();
        framing.Apply(pipe.Reader);

        await framing.ReadAsync(new byte[10], CancellationToken.None);

        var memory = await framing.ReadToEndAsync(CancellationToken.None);

        CollectionAssert.AreEqual(payload[10..], memory.ToArray());
        Assert.AreEqual(payload.Length, framing.Position);
    }

    [TestMethod]
    public async Task TestReadIntoEmptyBuffer()
    {
        var (_, framing) = await CreateAsync(Body);

        Assert.AreEqual(0, await framing.ReadAsync(Memory<byte>.Empty, CancellationToken.None));
    }

    [TestMethod]
    public async Task TestReadToEnd()
    {
        var (pipe, framing) = await CreateAsync(Body + "NEXT");

        var memory = await framing.ReadToEndAsync(CancellationToken.None);

        Assert.AreEqual("Hello World", Encoding.ASCII.GetString(memory.Span));
        Assert.AreEqual(11, framing.Position);

        Assert.IsTrue(await framing.SkipAsync());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestReadToEndAfterPartialRead()
    {
        var (_, framing) = await CreateAsync(Body);

        await framing.ReadAsync(new byte[2], CancellationToken.None);

        var memory = await framing.ReadToEndAsync(CancellationToken.None);

        Assert.AreEqual("llo World", Encoding.ASCII.GetString(memory.Span));
    }

    [TestMethod]
    public async Task TestReadAsDataArrives()
    {
        var pipe = new Pipe();

        var framing = new ChunkedFraming();
        framing.Apply(pipe.Reader);

        var pending = framing.ReadToEndAsync(CancellationToken.None);

        foreach (var part in new[] { "5\r\nHel", "lo\r\n6\r\n World\r", "\n0\r\n", "\r\n" })
        {
            await pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes(part));
        }

        var memory = await pending;

        Assert.AreEqual("Hello World", Encoding.ASCII.GetString(memory.Span));
    }

    [TestMethod]
    public async Task TestTruncatedBodyFails()
    {
        var (_, framing) = await CreateAsync("5\r\nHel");

        var e = await Assert.ThrowsExactlyAsync<ProviderException>(async () => await framing.ReadAsync(new byte[16], CancellationToken.None));

        Assert.AreEqual(ResponseStatus.BadRequest, e.Status);
    }

    [TestMethod]
    public async Task TestMissingBodyFails()
    {
        var (_, framing) = await CreateAsync("");

        await Assert.ThrowsExactlyAsync<ProviderException>(async () => await framing.ReadAsync(new byte[16], CancellationToken.None));
    }

    [TestMethod]
    public async Task TestMalformedBodyFails()
    {
        var (_, framing) = await CreateAsync("X\r\nHello\r\n0\r\n\r\n");

        var e = await Assert.ThrowsExactlyAsync<ProviderException>(async () => await framing.ReadToEndAsync(CancellationToken.None));

        Assert.AreEqual(ResponseStatus.BadRequest, e.Status);
    }

    [TestMethod]
    public async Task TestSkipWithinChunk()
    {
        var (pipe, framing) = await CreateAsync(Body + "NEXT");

        await framing.ReadAsync(new byte[2], CancellationToken.None);

        Assert.IsTrue(await framing.SkipAsync());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestSkipUnread()
    {
        var (pipe, framing) = await CreateAsync(Body + "NEXT");

        Assert.IsTrue(await framing.SkipAsync());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestSkipAsDataArrives()
    {
        var pipe = new Pipe();

        var framing = new ChunkedFraming();
        framing.Apply(pipe.Reader);

        await pipe.Writer.WriteAsync("5\r\nHello\r\n6\r\n Wo"u8.ToArray());

        var pending = framing.SkipAsync();

        await pipe.Writer.WriteAsync("rld\r\n0\r\n\r\nNEXT"u8.ToArray());
        await pipe.Writer.CompleteAsync();

        Assert.IsTrue(await pending);

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestSkipTruncatedBody()
    {
        var (_, framing) = await CreateAsync("5\r\nHel");

        Assert.IsFalse(await framing.SkipAsync());
    }

    [TestMethod]
    public async Task TestSkipMalformedBody()
    {
        var (_, framing) = await CreateAsync("X\r\n");

        Assert.IsFalse(await framing.SkipAsync());
    }

    [TestMethod]
    public async Task TestReuseAfterReset()
    {
        var (_, framing) = await CreateAsync(Body);

        await framing.ReadAsync(new byte[2], CancellationToken.None);

        framing.Reset();

        Assert.AreEqual(0, framing.Position);

        var pipe = new Pipe();

        await pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes(Body));
        await pipe.Writer.CompleteAsync();

        framing.Apply(pipe.Reader);

        var memory = await framing.ReadToEndAsync(CancellationToken.None);

        Assert.AreEqual("Hello World", Encoding.ASCII.GetString(memory.Span));
    }

    private static async Task<(Pipe, ChunkedFraming)> CreateAsync(string content)
    {
        var pipe = new Pipe();

        await pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes(content));
        await pipe.Writer.CompleteAsync();

        var framing = new ChunkedFraming();
        framing.Apply(pipe.Reader);

        return (pipe, framing);
    }

}
