using System.Net;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

using Host = GenHTTP.Engine.Ioxide.Host;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// The request shape the handler chain sees for one HTTP/2 stream: its query split out of the path,
/// a body that can be fetched once and decorated before it is, and the connection-level operations
/// this transport does not offer. The response version is asserted to be HTTP/2, so a fall back to
/// HTTP/1.1 - which builds a different request - fails the test rather than passing it wrongly.
/// </summary>
[TestClass]
public sealed class StreamedRequestTests
{

    [IoxideTestMethod]
    public async Task TestQueryIsSplitOutOfThePath()
    {
        // Covers the parser's branches at once: a normal pair, an empty pair that is skipped, a name
        // with no '=', and a name whose value is empty.
        var (body, version) = await SendAsync(DescribeQuery, HttpMethod.Get, "/?a=1&&b=2&flag&c=");

        Assert.AreEqual(HttpVersion.Version20, version);
        Assert.AreEqual("a=1|b=2|flag=|c=", body);
    }

    [IoxideTestMethod]
    public async Task TestPathWithoutAQueryHasNoParameters()
    {
        var (body, version) = await SendAsync(DescribeQuery, HttpMethod.Get, "/plain");

        Assert.AreEqual(HttpVersion.Version20, version);
        Assert.AreEqual(string.Empty, body);
    }

    [IoxideTestMethod]
    public async Task TestBodylessRequestHasNoBody()
    {
        var (body, version) = await SendAsync(
            static request => new(request.GetBody() is null ? "no-body" : "has-body"),
            HttpMethod.Get, "/");

        Assert.AreEqual(HttpVersion.Version20, version);
        Assert.AreEqual("no-body", body);
    }

    [IoxideTestMethod]
    public async Task TestBodyCanOnlyBeFetchedOnce()
    {
        var (body, version) = await SendAsync(FetchBodyTwice, HttpMethod.Post, "/", "some body"u8.ToArray());

        Assert.AreEqual(HttpVersion.Version20, version);
        Assert.AreEqual("refused", body);
    }

    [IoxideTestMethod]
    public async Task TestBodyCanBeWrappedBeforeItIsRead()
    {
        // WrapBody registers a decorator that GetBody then applies - here one that upper-cases the body.
        var (body, version) = await SendAsync(ReadThroughWrapper, HttpMethod.Post, "/", "hello"u8.ToArray());

        Assert.AreEqual(HttpVersion.Version20, version);
        Assert.AreEqual("HELLO", body);
    }

    [IoxideTestMethod]
    public async Task TestUpgradeIsNotSupported()
    {
        // HTTP/2 carries its own stream multiplexing, so there is no connection to hand back.
        var (body, version) = await SendAsync(TryUpgrade, HttpMethod.Get, "/");

        Assert.AreEqual(HttpVersion.Version20, version);
        Assert.AreEqual("not-supported", body);
    }

    #region Probes

    private static ValueTask<string> DescribeQuery(IRequest request)
    {
        var entries = new List<string>();

        for (var i = 0; i < request.Header.Query.Count; i++)
        {
            var entry = request.Header.Query.GetStringEntry(i);
            entries.Add($"{entry.Key}={entry.Value}");
        }

        return new(string.Join('|', entries));
    }

    private static ValueTask<string> FetchBodyTwice(IRequest request)
    {
        request.GetBody();

        try
        {
            request.GetBody();
            return new("not-enforced");
        }
        catch (InvalidOperationException)
        {
            return new("refused");
        }
    }

    private static async ValueTask<string> ReadThroughWrapper(IRequest request)
    {
        request.WrapBody(inner => new UpperCasingBody(inner));

        var body = request.GetBody()!;

        var bytes = (await body.AsMemoryAsync()).ToArray();

        return Encoding.ASCII.GetString(bytes);
    }

    private static ValueTask<string> TryUpgrade(IRequest request)
    {
        try
        {
            request.Upgrade();
            return new("upgraded");
        }
        catch (NotSupportedException)
        {
            return new("not-supported");
        }
    }

    #endregion

    #region Infrastructure

    private static async Task<(string Body, Version Version)> SendAsync(Func<IRequest, ValueTask<string>> probe,
        HttpMethod method, string pathAndQuery, byte[]? requestBody = null)
    {
        var port = (ushort)TestHost.NextPort();

        var server = Host.Create()
                         .Handler(new ProbeHandler(probe))
                         .Bind(IPAddress.Loopback, port, HttpProtocols.Http2);

        await server.StartAsync();

        try
        {
            using var socketsHandler = new SocketsHttpHandler();

            using var client = new HttpClient(socketsHandler)
            {
                DefaultRequestVersion = HttpVersion.Version20,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
                Timeout = TimeSpan.FromSeconds(10),
            };

            using var request = new HttpRequestMessage(method, $"http://localhost:{port}{pathAndQuery}");

            if (requestBody is not null)
            {
                request.Content = new ByteArrayContent(requestBody);
            }

            using var response = await client.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            return (await response.Content.ReadAsStringAsync(), response.Version);
        }
        finally
        {
            await server.StopAsync();
        }
    }

    /// <summary>Runs the probe against the request and returns whatever string it produced.</summary>
    private sealed class ProbeHandler(Func<IRequest, ValueTask<string>> probe) : IHandler
    {
        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var result = await probe(request);

            return request.Respond().Content(result).Build();
        }
    }

    /// <summary>A body decorator that upper-cases whatever the inner body yields.</summary>
    private sealed class UpperCasingBody(IRequestBody inner) : IRequestBody
    {
        public Stream AsStream() => throw new NotSupportedException();

        public async ValueTask<ReadOnlyMemory<byte>> AsMemoryAsync()
        {
            var bytes = (await inner.AsMemoryAsync()).ToArray();

            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)char.ToUpperInvariant((char)bytes[i]);
            }

            return bytes;
        }
    }

    #endregion

}
