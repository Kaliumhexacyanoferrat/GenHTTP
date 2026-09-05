using System.Net;
using System.Net.Quic;
using System.Net.Security;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using Host = GenHTTP.Engine.Ioxide.Host;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// Response header names and bodies as HTTP/2 and HTTP/3 require them.
///
/// HTTP field names are case-insensitive everywhere a handler can see, and GenHTTP's own
/// KnownHeaders table is written in the canonical HTTP/1.1 casing - so a handler setting "Vary" or
/// "Cache-Control" is doing nothing wrong. Both binary wires disagree: RFC 9113 8.2.1 and RFC 9114
/// 4.2 make an uppercase letter in a field name malformed, and a peer may reject the whole message
/// over it. That arrives as a clean 200 with an empty body, because the status is parsed before the
/// offending field - which is why this is asserted on the received bytes and the received casing,
/// and not just on the status code.
/// </summary>
[TestClass]
public sealed class StreamedHeaderCaseTests
{
    // Big enough to cross several DATA frames, so a body that is dropped cannot be mistaken for a
    // body that happened to be short.
    private const int BodyBytes = 32 * 1024;

    [IoxideTestMethod]
    public async Task TestHttp2LowercasesHandlerHeaders()
        => await AssertServed(HttpVersion.Version20, HttpProtocols.Http1AndHttp2);

    [IoxideTestMethod]
    public async Task TestHttp3LowercasesHandlerHeaders()
    {
        if (!QuicConnection.IsSupported)
        {
            Assert.Inconclusive("QUIC is not supported on this machine (msquic missing).");
        }

        await AssertServed(HttpVersion.Version30, HttpProtocols.Http3);
    }

    [IoxideTestMethod]
    public async Task TestHttp1KeepsCanonicalCasing()
    {
        // The other half of the rule: HTTP/1.1 has no such requirement, and the canonical casing is
        // the convention there. Normalising for the binary wires must not reach back into it.
        var port = (ushort)TestHost.NextPort();

        var server = Build().Bind(IPAddress.Loopback, port, httpProtocols: HttpProtocols.Http1);

        await server.StartAsync();

        try
        {
            using var client = Client(HttpVersion.Version11);

            using var response = await client.GetAsync($"http://localhost:{port}/file");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(response.Headers.TryGetValues("Vary", out _), "Vary was served");
        }
        finally
        {
            await server.StopAsync();
        }
    }

    private static async Task AssertServed(Version version, HttpProtocols protocols)
    {
        var secure = version == HttpVersion.Version30;

        var (certificate, key) = IoxideCertificates.Create("localhost", IoxideCertificates.Isolated());

        var port = (ushort)TestHost.NextPort();

        var host = Build();

        var server = secure
            ? host.Bind(IPAddress.Loopback, port, new FileCertificateProvider(certificate, key), httpProtocols: protocols)
            : host.Bind(IPAddress.Loopback, port, httpProtocols: protocols);

        await server.StartAsync();

        try
        {
            using var client = Client(version);

            var scheme = secure ? "https" : "http";

            using var response = await client.GetAsync($"{scheme}://localhost:{port}/file");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(version, response.Version);

            // The body has to survive the header block, which it does not if the peer rejected it.
            var body = await response.Content.ReadAsByteArrayAsync();

            Assert.AreEqual(BodyBytes, body.Length);
            Assert.AreEqual((byte)'x', body[0]);
            Assert.AreEqual((byte)'x', body[^1]);

            // .NET canonicalises the header names it knows, so its own casing proves nothing. The
            // custom name is the one whose received spelling still reflects the wire.
            var custom = response.Headers.Concat(response.Content.Headers)
                                 .Select(h => h.Key)
                                 .Single(k => k.Equals("x-test-header", StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual("x-test-header", custom, "the field name reached the peer lowercase");
        }
        finally
        {
            await server.StopAsync();
        }
    }

    private static HttpClient Client(Version version)
    {
        var handler = new SocketsHttpHandler
        {
            SslOptions = new SslClientAuthenticationOptions { RemoteCertificateValidationCallback = (_, _, _, _) => true },
        };

        return new HttpClient(handler)
        {
            DefaultRequestVersion = version,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
        };
    }

    // Canonical casing on the way in, exactly as a handler or a GenHTTP module writes it.
    private static IServerHost Build()
        => Host.Create()
               .Handler(Layout.Create()
                              .Add("file", Inline.Create()
                                                 .Get((IRequest request) => request.Respond()
                                                                                   .Header("Vary", "Accept-Encoding")
                                                                                   .Header("X-Test-Header", "AbC")
                                                                                   .Content(new string('x', BodyBytes), ContentType.TextPlain)
                                                                                   .Build())));
}
