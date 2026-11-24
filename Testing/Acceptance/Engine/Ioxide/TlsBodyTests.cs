using System.Net;
using System.Net.Security;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Testing.Acceptance.Engine.Body;

using Host = GenHTTP.Engine.Ioxide.Host;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// Request bodies over TLS, streamed straight back. The TLS reader decrypts into a pipe that reuses
/// its segments once they have been read, so this is where a body reader that still touches memory
/// it has handed back shows: an echoed chunked body used to come back short.
/// </summary>
[TestClass]
public sealed class TlsBodyTests
{
    private static readonly int[] Sizes = [64 * 1024, 100 * 1024, 1024 * 1024];

    private static readonly int[] Writes = [1, 100, 4095, 4097, 16_384, 65_536, 100_000];

    [IoxideTestMethod]
    public async Task TestChunkedBodiesOverHttp1()
    {
        await RunAsync(HttpProtocols.Http1, HttpVersion.Version11, chunked: true);
    }

    [IoxideTestMethod]
    public async Task TestBodiesOverHttp2()
    {
        // With a Content-Length: the engine treats an HTTP/2 request without one as having no body
        await RunAsync(HttpProtocols.Http1AndHttp2, HttpVersion.Version20, chunked: false);
    }

    private static async Task RunAsync(HttpProtocols protocols, Version version, bool chunked)
    {
        var (certificate, key) = IoxideCertificates.Create("localhost", IoxideCertificates.Isolated());

        var port = (ushort)TestHost.NextPort();

        var server = Host.Create()
                         .Handler(new StreamingEchoHandler().Wrap())
                         .Bind(IPAddress.Loopback, port, new FileCertificateProvider(certificate, key), httpProtocols: protocols);

        await server.StartAsync();

        try
        {
            using var handler = new SocketsHttpHandler
            {
                SslOptions = new SslClientAuthenticationOptions { RemoteCertificateValidationCallback = (_, _, _, _) => true },
            };

            using var client = new HttpClient(handler);

            // Each size a few times over, as how the data happens to arrive decides what the reader sees
            foreach (var size in Sizes)
            {
                for (var attempt = 1; attempt <= 5; attempt++)
                {
                    var payload = new byte[size];
                    new Random(size + attempt).NextBytes(payload);

                    var request = new HttpRequestMessage(HttpMethod.Post, $"https://localhost:{port}/")
                    {
                        Content = new UnevenContent(payload, Writes, declareLength: !chunked),
                        Version = version,
                        VersionPolicy = HttpVersionPolicy.RequestVersionExact
                    };

                    if (chunked)
                    {
                        request.Headers.TransferEncodingChunked = true;
                    }

                    using var response = await client.SendAsync(request);

                    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                    Assert.AreEqual(version, response.Version);

                    var echoed = await response.Content.ReadAsByteArrayAsync();

                    Assert.IsTrue(payload.AsSpan().SequenceEqual(echoed), $"{size} bytes, attempt {attempt}: {echoed.Length} came back");
                }
            }
        }
        finally
        {
            await server.StopAsync();
        }
    }

}
