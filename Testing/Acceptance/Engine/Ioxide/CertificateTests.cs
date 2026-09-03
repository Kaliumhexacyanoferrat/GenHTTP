using System.Net;
using System.Net.Sockets;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using Host = GenHTTP.Engine.Ioxide.Host;
using IoxideServer = GenHTTP.Engine.Ioxide.Infrastructure.Server;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// Which certificate a secure port serves, and what happens when it has none or gets a new one.
/// </summary>
[TestClass]
public sealed class CertificateTests
{

    [TestMethod]
    public async Task TestCertificateIsChosenByTheNameTheClientAsksFor()
    {
        var dir = IoxideCertificates.Isolated();

        var (defaultCertificate, defaultKey) = IoxideCertificates.Create("localhost", dir);

        var certificates = new HostCertificateProvider(defaultCertificate, defaultKey);

        var (alpha, alphaKey) = IoxideCertificates.Create("alpha.localhost", dir);

        certificates.Add("alpha.localhost", alpha, alphaKey);

        var port = (ushort)TestHost.NextPort();

        var server = Build().Bind(IPAddress.Loopback, port, certificates);

        await server.StartAsync();

        try
        {
            Assert.AreEqual("CN=alpha.localhost", await IoxideCertificates.SubjectAsync(port, "alpha.localhost"));

            // A name the table does not hold is served the default rather than refused: aborting
            // would leave the client with a connection error instead of a certificate to reason about.
            Assert.AreEqual("CN=localhost", await IoxideCertificates.SubjectAsync(port, "unknown.localhost"));
            Assert.AreEqual("CN=localhost", await IoxideCertificates.SubjectAsync(port, "localhost"));
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [TestMethod]
    public async Task TestSecurePortWithoutACertificateClosesRatherThanHangs()
    {
        // The port stays advertised as secure so upgrade redirects still work, but it cannot
        // handshake - so the connection is FINed and the client fails fast.
        var port = (ushort)TestHost.NextPort();

        var server = Build().Bind(IPAddress.Loopback, port, new NoCertificateProvider());

        await server.StartAsync();

        try
        {
            using var probe = new TcpClient();

            await probe.ConnectAsync(IPAddress.Loopback, port).WaitAsync(TimeSpan.FromSeconds(5));

            var read = probe.GetStream().ReadAsync(new byte[1]).AsTask();

            Assert.AreSame(read, await Task.WhenAny(read, Task.Delay(TimeSpan.FromSeconds(5))),
                           "the connection was left open instead of being closed");

            Assert.AreEqual(0, await read, "the port answered instead of closing");
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [TestMethod]
    public async Task TestCertificatesRotateOnARunningServer()
    {
        var dir = IoxideCertificates.Isolated();

        var (certificate, key) = IoxideCertificates.Create("localhost", dir);

        var port = (ushort)TestHost.NextPort();

        var server = Build().Bind(IPAddress.Loopback, port, new FileCertificateProvider(certificate, key));

        await server.StartAsync();

        try
        {
            var before = await IoxideCertificates.SerialAsync(port);

            // What an ACME hook does: rewrite the PEM the provider already names, then install it.
            IoxideCertificates.Create("localhost", dir);

            (server.Instance as IoxideServer)?.ReloadCertificates();

            var after = await IoxideCertificates.SerialAsync(port);

            Assert.AreNotEqual(before, after, "the port still serves the certificate it started with");
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [TestMethod]
    public async Task TestRotationNeedsASecureEndpoint()
    {
        var port = (ushort)TestHost.NextPort();

        var server = Build().Bind(IPAddress.Loopback, port);

        await server.StartAsync();

        try
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => (server.Instance as IoxideServer)!.ReloadCertificates());
        }
        finally
        {
            await server.StopAsync();
        }
    }

    private static IServerHost Build()
        => Host.Create().Handler(Layout.Create().Add("ok", Content.From(Resource.FromString("ok"))));

    /// <summary>Answers with nothing, which is how a port ends up secure but unable to handshake.</summary>
    private sealed class NoCertificateProvider : ICertificateProvider
    {
        public System.Security.Cryptography.X509Certificates.X509Certificate2? Provide(string? host) => null;
    }

}
