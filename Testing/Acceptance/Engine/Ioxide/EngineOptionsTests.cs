using System.Net;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using GenHTTP.Testing.Acceptance.Utilities;

using Host = GenHTTP.Engine.Ioxide.Host;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// The combinations the engine refuses when a server starts, rather than serving something the
/// binding did not ask for.
/// </summary>
[TestClass]
public sealed class EngineOptionsTests
{

    [TestMethod]
    public async Task TestOnePortCarriesOneEndpoint()
    {
        // A connection is matched to its endpoint by the port it arrived on, so a port bound twice
        // has no answer.
        var port = (ushort)TestHost.NextPort();

        await Assert.ThrowsExactlyAsync<NotSupportedException>(async () =>
        {
            var server = Build().Bind(IPAddress.Loopback, port)
                                            .Bind(IPAddress.Loopback, port);

            await server.StartAsync();
        });
    }

    [TestMethod]
    public async Task TestOnlyOneEndpointMayServeHttp3()
    {
        // The engine binds a single QUIC listener, so a second HTTP/3 port cannot be honoured.
        var first = (ushort)TestHost.NextPort();
        var second = (ushort)TestHost.NextPort();

        using var certificate = await Security.GetCertificateAsync();

        await Assert.ThrowsExactlyAsync<NotSupportedException>(async () =>
        {
            var server = Build(new EngineOptions
                                     {
                                         ProtocolsByPort =
                                         {
                                             [first] = Protocols.All,
                                             [second] = Protocols.All,
                                         },
                                     })
                                     .Bind(IPAddress.Loopback, first, certificate)
                                     .Bind(IPAddress.Loopback, second, certificate);

            await server.StartAsync();
        });
    }

    [TestMethod]
    public async Task TestRequiringAClientCertificateNeedsAnchors()
    {
        // Requiring a certificate with nothing to validate it against would ask every client for
        // one, refuse those without, then accept whatever the rest sent - authenticated in name only.
        var port = (ushort)TestHost.NextPort();

        using var certificate = await Security.GetCertificateAsync();

        await Assert.ThrowsExactlyAsync<NotSupportedException>(async () =>
        {
            var server = Build().Bind(IPAddress.Loopback, port, certificate,
                                                  certificateValidator: new AnchorlessValidator());

            await server.StartAsync();
        });
    }

    [TestMethod]
    public async Task TestEveryEndpointSharesOneDualStackMode()
    {
        // One reactor binds every listener, so the mode cannot differ per endpoint.
        var first = (ushort)TestHost.NextPort();
        var second = (ushort)TestHost.NextPort();

        await Assert.ThrowsExactlyAsync<NotSupportedException>(async () =>
        {
            var server = Build().Bind(IPAddress.Loopback, first, dualStack: true)
                                            .Bind(IPAddress.Loopback, second, dualStack: false);

            await server.StartAsync();
        });
    }

    [TestMethod]
    public async Task TestAPortMustServeSomething()
    {
        var port = (ushort)TestHost.NextPort();

        await Assert.ThrowsExactlyAsync<NotSupportedException>(async () =>
        {
            var server = Build(new EngineOptions { ProtocolsByPort = { [port] = 0 } })
                                     .Bind(IPAddress.Loopback, port);

            await server.StartAsync();
        });
    }

    [TestMethod]
    public async Task TestHttp3NeedsTheCertificateAsFiles()
    {
        // ngtcp2 loads PEM by path and the engine will not write a private key out on your behalf.
        var port = (ushort)TestHost.NextPort();

        using var certificate = await Security.GetCertificateAsync();

        await Assert.ThrowsExactlyAsync<NotSupportedException>(async () =>
        {
            var server = Build(new EngineOptions { ProtocolsByPort = { [port] = Protocols.Http3 } })
                                     .Bind(IPAddress.Loopback, port, certificate);

            await server.StartAsync();
        });
    }

    private static IServerHost Build(EngineOptions? options = null)
        => Host.Create(options: options)
               .Handler(Layout.Create().Add("ok", Content.From(Resource.FromString("ok"))));

    /// <summary>Demands a client certificate while naming nothing to validate one against.</summary>
    private sealed class AnchorlessValidator : ICertificateValidator
    {
        public bool RequireCertificate => true;

        public X509RevocationMode RevocationCheck => X509RevocationMode.NoCheck;

        public bool Validate(X509Certificate? certificate, X509Chain? chain, System.Net.Security.SslPolicyErrors policyErrors) => true;
    }

}
