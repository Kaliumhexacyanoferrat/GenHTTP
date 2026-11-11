using System.Net;
using System.Net.Security;
using System.Net.Quic;
using System.Net.Sockets;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using Host = GenHTTP.Engine.Ioxide.Host;
using IoxideServer = GenHTTP.Engine.Ioxide.Infrastructure.Server;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// HTTP/3 over QUIC, which shares a port number with the TCP transports but not a socket.
/// </summary>
[TestClass]
public sealed class Http3Tests
{

    [IoxideTestMethod]
    public async Task TestHttp3ServesRequests()
    {
        if (!QuicConnection.IsSupported)
        {
            Assert.Inconclusive("QUIC is not supported on this machine (msquic missing).");
        }

        var (certificate, key) = IoxideCertificates.Create("localhost", IoxideCertificates.Isolated());

        var port = (ushort)TestHost.NextPort();

        var server = Build().Bind(IPAddress.Loopback, port, new FileCertificateProvider(certificate, key),
                                  httpProtocols: HttpProtocols.Http3);

        await server.StartAsync();

        try
        {
            using var handler = new SocketsHttpHandler
            {
                SslOptions = new SslClientAuthenticationOptions { RemoteCertificateValidationCallback = (_, _, _, _) => true },
            };

            using var client = new HttpClient(handler)
            {
                DefaultRequestVersion = HttpVersion.Version30,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
            };

            using var response = await client.GetAsync($"https://localhost:{port}/ok");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(HttpVersion.Version30, response.Version);
            Assert.AreEqual("ok", await response.Content.ReadAsStringAsync());
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [IoxideTestMethod]
    public async Task TestHttp3OnlyBindsNoTcpListener()
    {
        // Naming Http3 alone for a port is taken literally: ResolveTcpPorts leaves it out, so no
        // TCP listener is bound and no TLS service is built for it on any reactor.
        var (certificate, key) = IoxideCertificates.Create("localhost", IoxideCertificates.Isolated());

        var port = (ushort)TestHost.NextPort();

        var server = Build().Bind(IPAddress.Loopback, port, new FileCertificateProvider(certificate, key),
                                  httpProtocols: HttpProtocols.Http3);

        await server.StartAsync();

        try
        {
            using var probe = new TcpClient();

            var connect = await Assert.ThrowsExactlyAsync<SocketException>(
                async () => await probe.ConnectAsync(IPAddress.Loopback, port).WaitAsync(TimeSpan.FromSeconds(5)));

            Assert.AreEqual(SocketError.ConnectionRefused, connect.SocketErrorCode);
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [IoxideTestMethod]
    public async Task TestHttp3OnlyStillRotatesItsCertificate()
    {
        // Rotation must not depend on a TCP TLS registry, which an HTTP/3-only server never builds.
        var dir = IoxideCertificates.Isolated();

        var (certificate, key) = IoxideCertificates.Create("localhost", dir);

        var port = (ushort)TestHost.NextPort();

        var server = Build().Bind(IPAddress.Loopback, port, new FileCertificateProvider(certificate, key),
                                  httpProtocols: HttpProtocols.Http3);

        await server.StartAsync();

        try
        {
            IoxideCertificates.Create("localhost", dir);

            (server.Instance as IoxideServer)!.ReloadCertificates();
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [IoxideTestMethod]
    public async Task TestHttp3SharesItsPortNumberWithTcp()
    {
        // The arrangement a browser can actually reach: TCP and QUIC on the same port number, since
        // browsers connect over TCP and move to QUIC only once Alt-Svc points them there.
        if (!QuicConnection.IsSupported)
        {
            Assert.Inconclusive("QUIC is not supported on this machine (msquic missing).");
        }

        var (certificate, key) = IoxideCertificates.Create("localhost", IoxideCertificates.Isolated());

        var port = (ushort)TestHost.NextPort();

        var server = Build().Bind(IPAddress.Loopback, port, new FileCertificateProvider(certificate, key),
                                  httpProtocols: HttpProtocols.All);

        await server.StartAsync();

        try
        {
            foreach (var version in new[] { HttpVersion.Version11, HttpVersion.Version20, HttpVersion.Version30 })
            {
                using var handler = new SocketsHttpHandler
                {
                    SslOptions = new SslClientAuthenticationOptions { RemoteCertificateValidationCallback = (_, _, _, _) => true },
                };

                using var client = new HttpClient(handler)
                {
                    DefaultRequestVersion = version,
                    DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
                };

                using var response = await client.GetAsync($"https://localhost:{port}/ok");

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"HTTP/{version} was not served");
                Assert.AreEqual(version, response.Version);
            }
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [IoxideTestMethod]
    public async Task TestHttp3StreamsTheRequestBody()
    {
        // Large enough to arrive in many chunks, which the handler reads through the body reader
        // ioxide hands the driver - an echo that comes back whole proves none went missing.
        if (!QuicConnection.IsSupported)
        {
            Assert.Inconclusive("QUIC is not supported on this machine (msquic missing).");
        }

        var (certificate, key) = IoxideCertificates.Create("localhost", IoxideCertificates.Isolated());

        var port = (ushort)TestHost.NextPort();

        var echo = Inline.Create().Post(async (Stream body) =>
        {
            var copy = new MemoryStream();
            var buffer = new byte[8192];

            // not CopyToAsync: its ConfigureAwait(false) resumes off the reactor, where the body
            // reader must not be called
            int read;

            while ((read = await body.ReadAsync(buffer)) > 0)
            {
                copy.Write(buffer, 0, read);
            }

            copy.Seek(0, SeekOrigin.Begin);

            return copy;
        });

        var server = Host.Create()
                         .Handler(echo)
                         .Bind(IPAddress.Loopback, port, new FileCertificateProvider(certificate, key),
                               httpProtocols: HttpProtocols.Http3);

        await server.StartAsync();

        try
        {
            using var handler = new SocketsHttpHandler
            {
                SslOptions = new SslClientAuthenticationOptions { RemoteCertificateValidationCallback = (_, _, _, _) => true },
            };

            using var client = new HttpClient(handler)
            {
                DefaultRequestVersion = HttpVersion.Version30,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
            };

            var payload = new byte[256 * 1024];
            new Random(42).NextBytes(payload);

            using var content = new ByteArrayContent(payload);

            using var response = await client.PostAsync($"https://localhost:{port}/", content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(HttpVersion.Version30, response.Version);

            CollectionAssert.AreEqual(payload, await response.Content.ReadAsByteArrayAsync());
        }
        finally
        {
            await server.StopAsync();
        }
    }

    private static IServerHost Build()
        => Host.Create()
               .Handler(Layout.Create().Add("ok", Content.From(Resource.FromString("ok"))));

}
