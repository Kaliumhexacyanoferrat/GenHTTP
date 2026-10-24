using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.ServerSentEvents;

using Host = GenHTTP.Engine.Ioxide.Host;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// How a streamed response ends over HTTP/2 and HTTP/3, where the end of the stream is all that tells
/// the client a response is complete: a client that leaves is noticed by the content still writing,
/// and a content that fails after the headers resets the stream rather than ending it cleanly.
/// </summary>
[TestClass]
public sealed class StreamedResponseEndTests
{

    [IoxideTestMethod]
    public async Task TestEventSourceNoticesTheClientLeftOverHttp2()
    {
        await AssertGeneratorEndsAsync(HttpVersion.Version20);
    }

    [IoxideTestMethod]
    public async Task TestEventSourceNoticesTheClientLeftOverHttp3()
    {
        await AssertGeneratorEndsAsync(HttpVersion.Version30);
    }

    [IoxideTestMethod]
    public async Task TestFailureAfterTheHeadersResetsTheHttp2Stream()
    {
        await AssertResponseFailsAsync(HttpVersion.Version20);
    }

    [IoxideTestMethod]
    public async Task TestFailureAfterTheHeadersResetsTheHttp3Stream()
    {
        await AssertResponseFailsAsync(HttpVersion.Version30);
    }

    [IoxideTestMethod]
    public async Task TestFailureBeforeTheHeadersIsStillA500OverHttp2()
    {
        // The pre-header path is unchanged: there is still time for an honest status.
        await using var server = await ServerAsync(new FailingHandler(), HttpVersion.Version20);

        using var client = server.CreateClient();

        using var response = await client.GetAsync(server.Url);

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.AreEqual(HttpVersion.Version20, response.Version);
    }

    #region Infrastructure

    // A source that sends until a send fails, which only happens once the engine reports the client gone.
    private static async Task AssertGeneratorEndsAsync(Version version)
    {
        var left = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var source = EventSource.Create()
                                .Generator(async c =>
                                {
                                    while (await c.CommentAsync("ping"))
                                    {
                                        await Task.Delay(50);
                                    }

                                    Assert.IsFalse(c.Connected);
                                    left.TrySetResult();
                                });

        await using var server = await ServerAsync(source.Build(), version);

        using (var client = server.CreateClient())
        {
            using var response = await client.GetAsync(server.Url, HttpCompletionOption.ResponseHeadersRead);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(version, response.Version);

            // wait for the first event, so the generator is running when the client leaves
            await using var stream = await response.Content.ReadAsStreamAsync();

            var buffer = new byte[1024];
            var received = "";

            while (!received.Contains(": ping"))
            {
                var read = await stream.ReadAsync(buffer);
                Assert.AreNotEqual(0, read);
                received += Encoding.ASCII.GetString(buffer, 0, read);
            }
        }

        var finished = await Task.WhenAny(left.Task, Task.Delay(TimeSpan.FromSeconds(10)));

        Assert.AreSame(left.Task, finished, $"The generator kept sending after the HTTP/{version} client left");
    }

    // The headers and the first part of the body are out when the content fails. The response has to
    // fail rather than end, since there is no content-length to tell the client the body is short.
    // Where it fails depends on the client: a reset may overtake the headers it has not read yet.
    private static async Task AssertResponseFailsAsync(Version version)
    {
        await using var server = await ServerAsync(new FailingMidBodyHandler(), version);

        using var client = server.CreateClient();

        var failure = await Assert.ThrowsAsync<Exception>(async () =>
        {
            using var response = await client.GetAsync(server.Url, HttpCompletionOption.ResponseHeadersRead);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(version, response.Version);

            await using var stream = await response.Content.ReadAsStreamAsync();

            await stream.CopyToAsync(Stream.Null);
        });

        Assert.IsTrue(failure is HttpRequestException or IOException, $"The response did not fail as a transfer: {failure}");
    }

    private static async Task<RunningServer> ServerAsync(IHandler handler, Version version)
    {
        var port = (ushort)TestHost.NextPort();

        IServerHost host;

        if (version == HttpVersion.Version30)
        {
            if (!QuicConnection.IsSupported)
            {
                Assert.Inconclusive("QUIC is not supported on this machine (msquic missing).");
            }

            var (certificate, key) = IoxideCertificates.Create("localhost", IoxideCertificates.Isolated());

            host = Host.Create().Handler(handler)
                       .Bind(IPAddress.Loopback, port, new FileCertificateProvider(certificate, key), httpProtocols: HttpProtocols.Http3);
        }
        else
        {
            host = Host.Create().Handler(handler)
                       .Bind(IPAddress.Loopback, port, HttpProtocols.Http2);
        }

        await host.StartAsync();

        return new RunningServer(host, port, version);
    }

    private sealed class RunningServer(IServerHost host, ushort port, Version version) : IAsyncDisposable
    {
        public string Url => version == HttpVersion.Version30 ? $"https://localhost:{port}/" : $"http://localhost:{port}/";

        public HttpClient CreateClient()
        {
            var handler = new SocketsHttpHandler
            {
                SslOptions = new SslClientAuthenticationOptions { RemoteCertificateValidationCallback = (_, _, _, _) => true },
            };

            return new HttpClient(handler)
            {
                DefaultRequestVersion = version,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
                Timeout = TimeSpan.FromSeconds(10),
            };
        }

        public async ValueTask DisposeAsync() => await host.StopAsync();
    }

    /// <summary>Answers with a body that fails once its first kilobyte is on the wire.</summary>
    private sealed class FailingMidBodyHandler : IHandler
    {
        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request)
            => new(request.Respond()
                          .Status(ResponseStatus.Ok)
                          .Content(new FailingMidBodyContent())
                          .Build());
    }

    /// <summary>Writes and flushes part of an unknown-length body, then fails.</summary>
    private sealed class FailingMidBodyContent : IResponseContent
    {
        public ulong? Length => null;

        public ContentType? Type => ContentType.ApplicationOctetStream;

        public ReadOnlyMemory<byte>? Encoding => null;

        public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong?)null);

        public async ValueTask WriteAsync(IResponseSink sink)
        {
            await sink.Stream.WriteAsync(new byte[1024]);
            await sink.Stream.FlushAsync();

            throw new InvalidOperationException("The content failed after its headers were sent");
        }
    }

    /// <summary>Fails before it has a response at all.</summary>
    private sealed class FailingHandler : IHandler
    {
        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request) => throw new InvalidOperationException("The handler failed");
    }

    #endregion

}
