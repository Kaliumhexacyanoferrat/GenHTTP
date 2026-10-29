using System.Buffers;
using System.IO.Pipelines;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types;

using Glyph11.Parser;
using Glyph11.Parser.UltraHardened;
using Glyph11.Pico;

using NSubstitute;

using static GenHTTP.Testing.Acceptance.Engine.Body.ContentLengthFramingTests;

namespace GenHTTP.Testing.Acceptance.Engine.Body;

[TestClass]
public sealed class RequestBodyLifecycleTests
{

    #region Finishing

    [TestMethod]
    public async Task TestFinishWithoutBody()
    {
        var (request, pipe) = await ParseAsync("GET / HTTP/1.1\r\nHost: localhost\r\n\r\nNEXT");

        Assert.IsFalse(request.HasBody);

        Assert.IsTrue(await request.FinishAsync());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestFinishSkipsUnreadContentLength()
    {
        var (request, pipe) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\n\r\nHelloNEXT");

        Assert.IsTrue(request.HasBody);

        Assert.IsTrue(await request.FinishAsync());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestFinishSkipsUnreadChunked()
    {
        var (request, pipe) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nTransfer-Encoding: chunked\r\n\r\n5\r\nHello\r\n0\r\n\r\nNEXT");

        Assert.IsTrue(await request.FinishAsync());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestFinishSkipsPartiallyReadBody()
    {
        var (request, pipe) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 10\r\n\r\nHelloWorldNEXT");

        var body = request.GetBody();

        Assert.IsNotNull(body);

        await body.AsStream().ReadExactlyAsync(new byte[3]);

        Assert.IsTrue(await request.FinishAsync());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestFinishReleasesMemoryView()
    {
        var (request, pipe) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\n\r\nHelloNEXT");

        var memory = await request.GetBody()!.AsMemoryAsync();

        Assert.AreEqual("Hello", Encoding.ASCII.GetString(memory.Span));

        Assert.IsTrue(await request.FinishAsync());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestFinishWithTruncatedBody()
    {
        var (request, _) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 10\r\n\r\nHello");

        Assert.IsFalse(await request.FinishAsync());
    }

    [TestMethod]
    public async Task TestFinishWithInvalidLength()
    {
        var (request, _) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nContent-Length: abc\r\n\r\nHello", validate: false);

        Assert.IsFalse(await request.FinishAsync());
    }

    [TestMethod]
    public async Task TestFinishWithUnsupportedEncoding()
    {
        var (request, _) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nTransfer-Encoding: gzip\r\n\r\nHello", validate: false);

        Assert.IsFalse(await request.FinishAsync());
    }

    [TestMethod]
    public async Task TestFinishAfterUpgrade()
    {
        var (request, _) = await ParseAsync("GET / HTTP/1.1\r\nHost: localhost\r\n\r\n");

        request.Upgrade();

        Assert.IsFalse(await request.FinishAsync());
    }

    #endregion

    #region Body access

    [TestMethod]
    public async Task TestNoBody()
    {
        var (request, _) = await ParseAsync("GET / HTTP/1.1\r\nHost: localhost\r\n\r\n");

        Assert.IsNull(request.GetBody());
    }

    [TestMethod]
    public async Task TestBodyCanOnlyBeFetchedOnce()
    {
        var (request, _) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\n\r\nHello");

        request.GetBody();

        Assert.ThrowsExactly<InvalidOperationException>(() => request.GetBody());
    }

    [TestMethod]
    public async Task TestHeaderIsRetained()
    {
        var (request, _) = await ParseAsync("POST /path HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\n\r\nHello");

        request.GetBody();

        Assert.AreEqual("/path", request.Header.Target.AsString(decode: false));
    }

    [TestMethod]
    public async Task TestHeaderIsReleased()
    {
        var (request, _) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\n\r\nHello");

        request.GetBody(HeaderAccess.Release);

        Assert.ThrowsExactly<InvalidOperationException>(() => request.Header);
    }

    [TestMethod]
    public async Task TestInvalidLength()
    {
        var (request, _) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nContent-Length: abc\r\n\r\nHello", validate: false);

        var e = Assert.ThrowsExactly<ProviderException>(() => request.GetBody());

        Assert.AreEqual(ResponseStatus.BadRequest, e.Status);
    }

    [TestMethod]
    public async Task TestUnsupportedEncoding()
    {
        var (request, _) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nTransfer-Encoding: gzip\r\n\r\nHello", validate: false);

        var e = Assert.ThrowsExactly<ProviderException>(() => request.GetBody());

        Assert.AreEqual(ResponseStatus.NotImplemented, e.Status);
    }

    [TestMethod]
    public async Task TestDeclaredLength()
    {
        var (request, _) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\n\r\nHello");

        var body = (RequestBody)request.GetBody()!;

        Assert.AreEqual(5UL, body.Length);
    }

    [TestMethod]
    public async Task TestMemoryContinuesStream()
    {
        var (request, _) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\n\r\nHello");

        var body = request.GetBody()!;

        var stream = body.AsStream();

        Assert.AreSame(stream, body.AsStream());

        Assert.AreEqual(2, await stream.ReadAsync(new byte[2]));

        Assert.AreEqual("llo", Encoding.ASCII.GetString((await body.AsMemoryAsync()).Span));
    }

    [TestMethod]
    public async Task TestStreamContinuesMemory()
    {
        var (request, _) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\n\r\nHello");

        var body = request.GetBody()!;

        var first = await body.AsMemoryAsync();
        var second = await body.AsMemoryAsync();

        Assert.IsTrue(first.Equals(second));

        Assert.AreEqual(0, await body.AsStream().ReadAsync(new byte[16]));
    }

    [TestMethod]
    public async Task TestMemoryOfDelayedBody()
    {
        var (request, pipe) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\n\r\n", complete: false);

        var body = request.GetBody()!;

        var pending = body.AsMemoryAsync();

        Assert.IsFalse(pending.IsCompleted);

        await pipe.Writer.WriteAsync("Hello"u8.ToArray());

        Assert.AreEqual("Hello", Encoding.ASCII.GetString((await pending).Span));
        Assert.AreEqual("Hello", Encoding.ASCII.GetString((await body.AsMemoryAsync()).Span));
    }

    [TestMethod]
    public async Task TestWrappedBodyIsDisposed()
    {
        var (request, _) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\n\r\nHello");

        request.WrapBody(inner => new DisposableBody(inner));

        var body = request.GetBody();

        var wrapped = Assert.IsInstanceOfType<DisposableBody>(body);

        request.Reset();

        Assert.IsTrue(wrapped.Disposed);
    }

    #endregion

    #region Upgrades

    [TestMethod]
    public async Task TestUpgrade()
    {
        var (request, pipe) = await ParseAsync("GET /socket HTTP/1.1\r\nHost: localhost\r\nContent-Length: 4\r\n\r\nNEXT");

        var reader = request.Upgrade();

        Assert.AreSame(pipe.Reader, reader);

        Assert.IsFalse(request.HasBody);
        Assert.AreEqual("/socket", request.Header.Target.AsString(decode: false));

        Assert.ThrowsExactly<InvalidOperationException>(() => request.GetBody());

        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestUpgradeAfterReleasingHeader()
    {
        var (request, _) = await ParseAsync("GET / HTTP/1.1\r\nHost: localhost\r\n\r\n");

        request.GetBody(HeaderAccess.Release);

        request.Upgrade();

        Assert.ThrowsExactly<InvalidOperationException>(() => request.Header);
    }

    #endregion

    #region Expect: 100-continue

    private const string Continue = "HTTP/1.1 100 Continue\r\n\r\n";

    private const string ExpectingRequest = "POST / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\nExpect: 100-continue\r\n\r\nHello";

    [TestMethod]
    public async Task TestContinueSentWithFirstRead()
    {
        var output = new Pipe();

        var (request, _) = await ParseAsync(ExpectingRequest, output: output.Writer);

        var body = request.GetBody()!;

        Assert.AreEqual("", Written(output));

        var memory = await body.AsMemoryAsync();

        Assert.AreEqual("Hello", Encoding.ASCII.GetString(memory.Span));
        Assert.AreEqual(Continue, Written(output));
    }

    [TestMethod]
    public async Task TestContinueSentOnce()
    {
        var output = new Pipe();

        var (request, _) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nTransfer-Encoding: chunked\r\nExpect: 100-continue\r\n\r\n2\r\nHe\r\n3\r\nllo\r\n0\r\n\r\n", output: output.Writer);

        var stream = request.GetBody()!.AsStream();

        var buffer = new byte[2];

        while (await stream.ReadAsync(buffer) > 0) { }

        Assert.AreEqual(Continue, Written(output));
    }

    [TestMethod]
    public async Task TestContinueIgnoresCase()
    {
        var output = new Pipe();

        var (request, _) = await ParseAsync(ExpectingRequest.Replace("100-continue", "100-Continue"), output: output.Writer);

        await request.GetBody()!.AsMemoryAsync();

        Assert.AreEqual(Continue, Written(output));
    }

    [TestMethod]
    public async Task TestNoContinueAfterResponseStarted()
    {
        var output = new Pipe();

        var (request, _) = await ParseAsync(ExpectingRequest, output: output.Writer);

        var body = request.GetBody()!;

        request.ResponseStarting();

        var memory = await body.AsMemoryAsync();

        Assert.AreEqual("Hello", Encoding.ASCII.GetString(memory.Span));
        Assert.AreEqual("", Written(output));
    }

    [TestMethod]
    public async Task TestNoContinueWhenSkipped()
    {
        var output = new Pipe();

        var (request, pipe) = await ParseAsync(ExpectingRequest + "NEXT", output: output.Writer);

        request.ResponseStarting();

        // the final response, which has not been flushed by the engine yet
        output.Writer.Write("HTTP/1.1 200 OK\r\n\r\n"u8);

        Assert.IsTrue(await request.FinishAsync());

        Assert.AreEqual("HTTP/1.1 200 OK\r\n\r\n", Written(output));
        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestNoContinueWhenSkippedAfterContinue()
    {
        var output = new Pipe();

        var (request, pipe) = await ParseAsync(ExpectingRequest + "NEXT", output: output.Writer);

        var stream = request.GetBody()!.AsStream();

        Assert.AreEqual(2, await stream.ReadAsync(new byte[2]));

        Assert.IsTrue(await request.FinishAsync());

        Assert.AreEqual(Continue, Written(output));
        Assert.AreEqual("NEXT", await RestAsync(pipe));
    }

    [TestMethod]
    public async Task TestNoContinueForHttp10()
    {
        var output = new Pipe();

        var (request, _) = await ParseAsync(ExpectingRequest.Replace("HTTP/1.1", "HTTP/1.0"), output: output.Writer);

        await request.GetBody()!.AsMemoryAsync();

        Assert.AreEqual("", Written(output));
    }

    [TestMethod]
    public async Task TestNoContinueWithoutBody()
    {
        var output = new Pipe();

        var (request, _) = await ParseAsync("POST / HTTP/1.1\r\nHost: localhost\r\nExpect: 100-continue\r\n\r\n", output: output.Writer);

        Assert.IsNull(request.GetBody());
        Assert.IsTrue(await request.FinishAsync());

        Assert.AreEqual("", Written(output));
    }

    [TestMethod]
    public async Task TestNoContinueWithoutWriter()
    {
        var (request, _) = await ParseAsync(ExpectingRequest);

        var memory = await request.GetBody()!.AsMemoryAsync();

        Assert.AreEqual("Hello", Encoding.ASCII.GetString(memory.Span));
    }

    [TestMethod]
    public async Task TestContinueNotCarriedOver()
    {
        var output = new Pipe();

        var (request, _) = await ParseAsync(ExpectingRequest, output: output.Writer);

        request.ResponseStarting();

        Assert.IsTrue(await request.FinishAsync());

        request.Reset();

        Written(output);

        var pipe = new Pipe();

        await pipe.Writer.WriteAsync("POST / HTTP/1.1\r\nHost: localhost\r\nContent-Length: 5\r\n\r\nHello"u8.ToArray());
        await pipe.Writer.CompleteAsync();

        var buffer = (await pipe.Reader.ReadAsync()).Buffer;

        Assert.IsTrue(UltraHardenedParser.TryExtractFullHeaderValidated(ref buffer, request.Source, ParserLimits.Default, out var bytesRead));

        request.Apply(Substitute.For<IServer>(), Substitute.For<IEndPoint>(), pipe.Reader, buffer.Slice(bytesRead + 1).Start, null, null, output.Writer);

        await request.GetBody()!.AsMemoryAsync();

        Assert.AreEqual("", Written(output));
    }

    /// <summary>
    /// Returns what has been flushed to the given output so far.
    /// </summary>
    private static string Written(Pipe output)
    {
        if (!output.Reader.TryRead(out var result))
        {
            return "";
        }

        var written = Encoding.ASCII.GetString(result.Buffer);

        output.Reader.AdvanceTo(result.Buffer.End);

        return written;
    }

    #endregion

    #region Without connection

    [TestMethod]
    public async Task TestWithoutConnection()
    {
        var request = new Request();

        request.Apply(Substitute.For<IServer>());

        Assert.IsFalse(request.HasBody);
        Assert.IsNull(request.GetBody());

        Assert.IsTrue(await request.FinishAsync());

        Assert.ThrowsExactly<InvalidOperationException>(() => request.Upgrade());
    }

    #endregion

    #region Helpers

    private static async Task<(Request, Pipe)> ParseAsync(string raw, bool complete = true, bool validate = true, PipeWriter? output = null)
    {
        var pipe = new Pipe();

        await pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes(raw));

        if (complete)
        {
            await pipe.Writer.CompleteAsync();
        }

        var request = new Request();

        var result = await pipe.Reader.ReadAsync();
        var buffer = result.Buffer;

        if (validate)
        {
            Assert.IsTrue(UltraHardenedParser.TryExtractFullHeaderValidated(ref buffer, request.Source, ParserLimits.Default, out var bytesRead));
            buffer = buffer.Slice(bytesRead + 1);
        }
        else
        {
            // picohttpparser does not validate the framing headers, so they reach the request as sent
            Assert.IsTrue(PicoParser.TryParse(buffer, request.Source, out var consumed));
            buffer = buffer.Slice(consumed + 1);
        }

        var endPoint = Substitute.For<IEndPoint>();

        request.Apply(Substitute.For<IServer>(), endPoint, pipe.Reader, buffer.Start, null, null, output);

        return (request, pipe);
    }

    private sealed class DisposableBody(IRequestBody inner) : IRequestBody, IDisposable
    {

        public bool Disposed { get; private set; }

        public Stream AsStream() => inner.AsStream();

        public ValueTask<ReadOnlyMemory<byte>> AsMemoryAsync() => inner.AsMemoryAsync();

        public void Dispose() => Disposed = true;

    }

    #endregion

}
