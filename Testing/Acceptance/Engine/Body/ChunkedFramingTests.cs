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
    public async Task TestReadWholeChunkStartingInLaterSegment()
    {
        // The second chunk starts in the pipe's second segment and ends in its third. Advancing past
        // it hands the second segment back to the pool, which resets it, so the length of the chunk
        // has to be known before.
        var first = new byte[6000];
        var second = new byte[6000];

        new Random(7).NextBytes(first);
        new Random(8).NextBytes(second);

        var pipe = new Pipe();

        await pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes($"{first.Length:X}\r\n"));
        await pipe.Writer.WriteAsync(first);
        await pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes($"\r\n{second.Length:X}\r\n"));
        await pipe.Writer.WriteAsync(second);
        await pipe.Writer.WriteAsync("\r\n0\r\n\r\nNEXT"u8.ToArray());
        await pipe.Writer.CompleteAsync();

        var framing = new ChunkedFraming();
        framing.Apply(pipe.Reader);

        var buffer = new byte[8192];

        Assert.AreEqual(first.Length, await framing.ReadAsync(buffer, CancellationToken.None));
        CollectionAssert.AreEqual(first, buffer[..first.Length]);

        Assert.AreEqual(second.Length, await framing.ReadAsync(buffer, CancellationToken.None));
        CollectionAssert.AreEqual(second, buffer[..second.Length]);

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

        var buffer = new byte[16];

        Assert.AreEqual(3, await framing.ReadAsync(buffer, CancellationToken.None));
        Assert.AreEqual("Hel", Encoding.ASCII.GetString(buffer, 0, 3));

        var e = await Assert.ThrowsExactlyAsync<ProviderException>(async () => await framing.ReadAsync(buffer, CancellationToken.None));

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

    #region Readers reusing their buffers

    [TestMethod]
    public async Task TestLargeBodyFromRecyclingReader()
    {
        // Chunks of uneven sizes, some larger than a read, through a reader that overwrites every
        // segment it is advanced past. However segments and reads fall, the payload comes back exactly.
        var payload = Payload(256 * 1024, 21);
        var body = Chunked(payload, [1, 100, 4095, 4096, 4097, 16_384, 65_536, 100_000]);

        foreach (var segmentSize in (int[])[1000, 4096, 16_384])
        {
            foreach (var bufferSize in (int[])[7, 3000, 8192, 70_000])
            {
                var layout = $"segments of {segmentSize} bytes, reads of {bufferSize}";

                var reader = new RecyclingPipeReader(body, segmentSize, segmentsPerRead: 3);

                var framing = new ChunkedFraming();
                framing.Apply(reader);

                var result = new MemoryStream();
                var buffer = new byte[bufferSize];

                int read;

                while ((read = await framing.ReadAsync(buffer, CancellationToken.None)) > 0)
                {
                    Assert.IsTrue(read <= bufferSize, $"{read} bytes read into {bufferSize} ({layout})");
                    result.Write(buffer, 0, read);
                }

                Assert.IsTrue(payload.AsSpan().SequenceEqual(result.ToArray()), layout);
                Assert.AreEqual(payload.Length, framing.Position, layout);

                Assert.IsTrue(await framing.SkipAsync(), layout);
                Assert.AreEqual("NEXT", reader.Rest(), layout);

                Assert.IsTrue(reader.Recycled > 0, layout);
            }
        }
    }

    [TestMethod]
    public async Task TestLargeBodyToEndFromRecyclingReader()
    {
        var payload = Payload(4 * 1024 * 1024, 22);

        var reader = new RecyclingPipeReader(Chunked(payload, [4096, 65_536, 4095, 100_000]), 4096, segmentsPerRead: 4);

        var framing = new ChunkedFraming();
        framing.Apply(reader);

        var body = await framing.ReadToEndAsync(CancellationToken.None);

        Assert.IsTrue(payload.AsSpan().SequenceEqual(body.Span));

        Assert.IsTrue(await framing.SkipAsync());
        Assert.AreEqual("NEXT", reader.Rest());
    }

    [TestMethod]
    public async Task TestSkipLargeBodyFromRecyclingReader()
    {
        var reader = new RecyclingPipeReader(Chunked(Payload(1024 * 1024, 23), [4095, 65_536, 1]), 4096, segmentsPerRead: 2);

        var framing = new ChunkedFraming();
        framing.Apply(reader);

        Assert.IsTrue(await framing.SkipAsync());
        Assert.AreEqual("NEXT", reader.Rest());
    }

    private static byte[] Payload(int length, int seed)
    {
        var payload = new byte[length];
        new Random(seed).NextBytes(payload);

        return payload;
    }

    /// <summary>
    /// The payload chunk-encoded in chunks of the given sizes, taken in turn, followed by the
    /// start of a next request.
    /// </summary>
    private static byte[] Chunked(byte[] payload, int[] chunkSizes)
    {
        var body = new MemoryStream();

        var offset = 0;
        var i = 0;

        while (offset < payload.Length)
        {
            var size = Math.Min(chunkSizes[i++ % chunkSizes.Length], payload.Length - offset);

            body.Write(Encoding.ASCII.GetBytes($"{size:X}\r\n"));
            body.Write(payload, offset, size);
            body.Write("\r\n"u8);

            offset += size;
        }

        body.Write("0\r\n\r\nNEXT"u8);

        return body.ToArray();
    }

    #endregion

}
