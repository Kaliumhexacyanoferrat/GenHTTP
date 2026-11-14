using System.IO.Pipelines;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types.Body;

namespace GenHTTP.Testing.Acceptance.Engine.Body;

[TestClass]
public sealed class ContentLengthFramingTests
{

    [TestMethod]
    public async Task TestReadStopsAtLength()
    {
        var (pipe, framing) = await CreateAsync("HelloNEXT", 5);

        var buffer = new byte[16];

        var read = await framing.ReadAsync(buffer, CancellationToken.None);

        Assert.AreEqual("Hello", Encoding.ASCII.GetString(buffer, 0, read));
        Assert.AreEqual(5, framing.Position);
        Assert.AreEqual(5, framing.Length);

        Assert.AreEqual(0, await framing.ReadAsync(buffer, CancellationToken.None));

        Assert.IsTrue(await framing.SkipAsync());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestReadIntoEmptyBuffer()
    {
        var (_, framing) = await CreateAsync("Hello", 5);

        Assert.AreEqual(0, await framing.ReadAsync(Memory<byte>.Empty, CancellationToken.None));
        Assert.AreEqual(0, framing.Position);
    }

    [TestMethod]
    public async Task TestReadWithSmallBuffer()
    {
        var (pipe, framing) = await CreateAsync("HelloNEXT", 5);

        var result = new StringBuilder();
        var buffer = new byte[2];

        int read;

        while ((read = await framing.ReadAsync(buffer, CancellationToken.None)) > 0)
        {
            result.Append(Encoding.ASCII.GetString(buffer, 0, read));
        }

        Assert.AreEqual("Hello", result.ToString());

        Assert.IsTrue(await framing.SkipAsync());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestReadTruncatedBodyFails()
    {
        var (_, framing) = await CreateAsync("abc", 5);

        var buffer = new byte[16];

        Assert.AreEqual(3, await framing.ReadAsync(buffer, CancellationToken.None));

        var e = await Assert.ThrowsExactlyAsync<ProviderException>(async () => await framing.ReadAsync(buffer, CancellationToken.None));

        Assert.AreEqual(ResponseStatus.BadRequest, e.Status);
    }

    [TestMethod]
    public async Task TestReadToEndKeepsSingleSegment()
    {
        var (pipe, framing) = await CreateAsync("HelloNEXT", 5);

        var memory = await framing.ReadToEndAsync(CancellationToken.None);

        Assert.AreEqual("Hello", Encoding.ASCII.GetString(memory.Span));
        Assert.AreEqual(5, framing.Position);

        Assert.IsTrue(await framing.SkipAsync());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestReadToEndLinearizesSegments()
    {
        var payload = new byte[32 * 1024];
        new Random(42).NextBytes(payload);

        var pipe = new Pipe();

        await pipe.Writer.WriteAsync(payload.AsMemory(0, 1024));

        var framing = new ContentLengthFraming();
        framing.Apply(pipe.Reader, payload.Length);

        var pending = framing.ReadToEndAsync(CancellationToken.None);

        await pipe.Writer.WriteAsync(payload.AsMemory(1024));
        await pipe.Writer.WriteAsync("NEXT"u8.ToArray());
        await pipe.Writer.CompleteAsync();

        var memory = await pending;

        CollectionAssert.AreEqual(payload, memory.ToArray());

        Assert.IsTrue(await framing.SkipAsync());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestReadToEndWithoutLength()
    {
        // nothing has been written and the pipe is still open, so this would hang if it tried to read
        var pipe = new Pipe();

        var framing = new ContentLengthFraming();
        framing.Apply(pipe.Reader, 0);

        var memory = await framing.ReadToEndAsync(CancellationToken.None);

        Assert.IsTrue(memory.IsEmpty);
        Assert.IsTrue(await framing.SkipAsync());
    }

    [TestMethod]
    public async Task TestReadToEndTruncatedBodyFails()
    {
        var (_, framing) = await CreateAsync("abc", 5);

        var e = await Assert.ThrowsExactlyAsync<ProviderException>(async () => await framing.ReadToEndAsync(CancellationToken.None));

        Assert.AreEqual(ResponseStatus.BadRequest, e.Status);
    }

    [TestMethod]
    public async Task TestSkipAfterPartialRead()
    {
        var (pipe, framing) = await CreateAsync("HelloWorldNEXT", 10);

        var buffer = new byte[3];

        await framing.ReadAsync(buffer, CancellationToken.None);

        Assert.IsTrue(await framing.SkipAsync());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestSkipWaitsForData()
    {
        var pipe = new Pipe();

        await pipe.Writer.WriteAsync("Hello"u8.ToArray());

        var framing = new ContentLengthFraming();
        framing.Apply(pipe.Reader, 10);

        var pending = framing.SkipAsync();

        await pipe.Writer.WriteAsync("WorldNEXT"u8.ToArray());
        await pipe.Writer.CompleteAsync();

        Assert.IsTrue(await pending);

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestSkipTruncatedBody()
    {
        var (_, framing) = await CreateAsync("abc", 5);

        Assert.IsFalse(await framing.SkipAsync());
    }

    private static async Task<(Pipe, ContentLengthFraming)> CreateAsync(string content, long length)
    {
        var pipe = new Pipe();

        await pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes(content));
        await pipe.Writer.CompleteAsync();

        var framing = new ContentLengthFraming();
        framing.Apply(pipe.Reader, length);

        return (pipe, framing);
    }

    internal static async Task<string> RestAsync(Pipe pipe)
    {
        var result = await pipe.Reader.ReadAsync();

        var rest = Encoding.ASCII.GetString(result.Buffer);

        pipe.Reader.AdvanceTo(result.Buffer.End);

        return rest;
    }

    #region Readers reusing their buffers

    [TestMethod]
    public async Task TestLargeBodyFromRecyclingReader()
    {
        // Through a reader that overwrites every segment it is advanced past: however segments
        // and reads fall, the payload comes back exactly.
        var payload = RandomPayload(256 * 1024, 31);
        var body = payload.Concat("NEXT"u8.ToArray()).ToArray();

        foreach (var segmentSize in (int[])[1000, 4096, 16_384])
        {
            foreach (var bufferSize in (int[])[7, 3000, 8192, 70_000])
            {
                var layout = $"segments of {segmentSize} bytes, reads of {bufferSize}";

                var reader = new RecyclingPipeReader(body, segmentSize, segmentsPerRead: 3);

                var framing = new ContentLengthFraming();
                framing.Apply(reader, payload.Length);

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
        var payload = RandomPayload(4 * 1024 * 1024, 32);

        var reader = new RecyclingPipeReader(payload.Concat("NEXT"u8.ToArray()).ToArray(), 4096, segmentsPerRead: 4);

        var framing = new ContentLengthFraming();
        framing.Apply(reader, payload.Length);

        var body = await framing.ReadToEndAsync(CancellationToken.None);

        Assert.IsTrue(payload.AsSpan().SequenceEqual(body.Span));

        Assert.IsTrue(await framing.SkipAsync());
        Assert.AreEqual("NEXT", reader.Rest());
    }

    private static byte[] RandomPayload(int length, int seed)
    {
        var payload = new byte[length];
        new Random(seed).NextBytes(payload);

        return payload;
    }

    #endregion

}
