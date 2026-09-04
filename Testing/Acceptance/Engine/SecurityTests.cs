using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Security.Providers;

using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class SecurityTests
{

    /// <summary>
    /// As a developer I would like to serve my application in a secure manner.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public Task TestSecure(TestEngine engine)
    {
        return RunSecure(async (_, sec) =>
        {
            using var client = TestHost.GetClient(ignoreSecurityErrors: true);

            using var response = await client.GetAsync($"https://localhost:{sec}");

            await response.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("Hello Alice!", await response.Content.ReadAsStringAsync());
        }, engine);
    }

    /// <summary>
    /// As a developer, I expect the server to redirect to a secure endpoint
    /// by default.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public Task TestDefaultRedirection(TestEngine engine)
    {
        return RunSecure(async (insec, sec) =>
        {
            using var client = TestHost.GetClient(followRedirects: false);

            using var response = await client.GetAsync($"http://localhost:{insec}");

            await response.AssertStatusAsync(HttpStatusCode.MovedPermanently);
            Assert.AreEqual($"https://localhost:{sec}/", response.Headers.GetValues("Location").First());
        }, engine);
    }

    /// <summary>
    /// As a developer, I expect HTTP requests not to be redirected if
    /// upgrades are allowed but not requested by the client.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public Task TestNoRedirectionWithAllowed(TestEngine engine)
    {
        return RunSecure(async (insec, _) =>
        {
            using var client = TestHost.GetClient(followRedirects: false);

            using var response = await client.GetAsync($"http://localhost:{insec}");

            await response.AssertStatusAsync(HttpStatusCode.OK);
        }, engine, SecureUpgrade.Allow);
    }

    /// <summary>
    /// As I developer, I expect requests to be upgraded if requested
    /// by the client.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public Task TestRedirectionWhenRequested(TestEngine engine)
    {
        return RunSecure(async (insec, sec) =>
        {
            using var client = TestHost.GetClient(followRedirects: false);

            var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:{insec}");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");

            using var response = await client.SendAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.TemporaryRedirect);

            Assert.AreEqual($"https://localhost:{sec}/", response.Headers.GetValues("Location").First());
            Assert.AreEqual("Upgrade-Insecure-Requests", response.Headers.GetValues("Vary").First());

            Assert.IsFalse(response.Headers.Contains("ETag"));
        }, engine, SecureUpgrade.Allow);
    }

    /// <summary>
    /// As the host of a web application, I want my application to enforce strict
    /// transport security, so that man-in-the-middle attacks can be avoided to some extent.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public Task TestTransportPolicy(TestEngine engine)
    {
        return RunSecure(async (insec, sec) =>
        {
            using var client = TestHost.GetClient(ignoreSecurityErrors: true);

            using var insecureResponse = await client.GetAsync($"http://localhost:{insec}");

            await insecureResponse.AssertStatusAsync(HttpStatusCode.OK);
            Assert.IsFalse(insecureResponse.Headers.Contains("Strict-Transport-Security"));

            using var secureResponse = await client.GetAsync($"https://localhost:{sec}");

            await secureResponse.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("max-age=31536000; includeSubDomains; preload", secureResponse.Headers.GetValues("Strict-Transport-Security").First());

        }, engine, SecureUpgrade.None);
    }

    /// <summary>
    /// As the operator of the server, I expect the server to resume
    /// normal operation after a security error has happened.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public Task TestSecurityError(TestEngine engine)
    {
        return RunSecure(async (_, sec) =>
        {
            await Assert.ThrowsExactlyAsync<HttpRequestException>(async () =>
            {
                using var client = TestHost.GetClient();

                using var failedResponse = await client.GetAsync($"https://localhost:{sec}");
            });

            using var client = TestHost.GetClient(ignoreSecurityErrors: true);
            using var response = await client.GetAsync($"https://localhost:{sec}");

            await response.AssertStatusAsync(HttpStatusCode.OK);
        }, engine);
    }

    /// <summary>
    /// As a web developer, I can decide not to return a certificate which will
    /// abort the server SSL handshake.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public Task TestNoCertificate(TestEngine engine)
    {
        return RunSecure(async (_, sec) =>
        {
            await Assert.ThrowsExactlyAsync<HttpRequestException>(async () =>
            {
                using var client = TestHost.GetClient(ignoreSecurityErrors: false);

                using var failedResponse = await client.GetAsync($"https://localhost:{sec}");
            });
        }, engine, host: "myserver");
    }

    /// <summary>
    /// The verdict an ICertificateValidator returns has to actually decide the connection. A
    /// validator that says no is the whole point of being asked, so a client it rejects must not
    /// reach the handler; one that says yes must.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestValidatorVerdictIsHonoured(TestEngine engine)
    {
        if (engine == TestEngine.Kestrel)
        {
            // Kestrel only calls ClientCertificateValidation when a certificate actually arrives, so
            // under AllowCertificate a client offering none is admitted without the validator being
            // asked at all. Its verdict is honoured, just never sought in this case.
            Assert.Inconclusive("Kestrel does not consult the validator when no client certificate is offered.");
        }

        await Assert.ThrowsExactlyAsync<HttpRequestException>(async () =>
        {
            using var response = await RunWithVerdictAsync(false, engine);
        });

        using var accepted = await RunWithVerdictAsync(true, engine);

        await accepted.AssertStatusAsync(HttpStatusCode.OK);
    }

    private static async Task<HttpResponseMessage> RunWithVerdictAsync(bool verdict, TestEngine engine)
    {
        var content = Layout.Create().Index(Content.From(Resource.FromString("Hello Alice!")));

        await using var runner = new TestHost(Layout.Create().Build(), false, engine: engine);

        var port = TestHost.NextPort();

        using var cert = await Security.GetCertificateAsync();

        var provider = CertificateProvider.From(cert);
        
        runner.Host.Handler(content)
              .Bind(IPAddress.Any, (ushort)port, provider, certificateValidator: new VerdictValidator(verdict));

        await runner.StartAsync();

        using var client = TestHost.GetClient(ignoreSecurityErrors: true);

        return await client.GetAsync($"https://localhost:{port}");
    }

    /// <summary>Answers every peer the same way, so only the verdict is under test.</summary>
    private sealed class VerdictValidator(bool verdict) : ICertificateValidator
    {
        // False: the engine must still ask, and a client offering nothing is what it is asked about.
        public bool RequireCertificate => false;

        public X509RevocationMode RevocationCheck => X509RevocationMode.NoCheck;

        public bool Validate(X509Certificate? certificate, X509Chain? chain, SslPolicyErrors policyErrors) => verdict;
    }

    private static async Task RunSecure(Func<ushort, ushort, Task> logic, TestEngine engine, SecureUpgrade? mode = null, string host = "localhost")
    {
        var content = Layout.Create().Index(Content.From(Resource.FromString("Hello Alice!")));

        await using var runner = new TestHost(Layout.Create().Build(), mode is null, engine: engine);

        var port = TestHost.NextPort();

        using var cert = await Security.GetCertificateAsync();

        // Serve the name the client actually presents (localhost) through a plain provider, so every
        // engine can answer - ioxide included, which resolves its certificate once at startup with no
        // host name and so needs one that answers a null host. A test that wants the handshake to abort
        // asks for a different name, leaving PickyCertificateProvider with nothing to offer localhost.
        ICertificateProvider certificates = host == "localhost"
            ? CertificateProvider.From(cert)
            : new PickyCertificateProvider(host, cert);

        runner.Host.Handler(content)
              .Bind(IPAddress.Any, (ushort)runner.Port)
              .Bind(IPAddress.Any, (ushort)port, certificates, SslProtocols.Tls12);

        if (mode is not null)
        {
            runner.Host.SecureUpgrade(mode.Value);
            runner.Host.StrictTransport(new StrictTransportPolicy(TimeSpan.FromDays(365), true, true));
        }

        await runner.StartAsync();

        await logic((ushort)runner.Port, (ushort)port);
    }

    /// <summary>
    /// Answers only for the one name it was given; Provide(null) and every other name return nothing, so
    /// a client asking for anything else finds no certificate and the handshake is refused.
    /// </summary>
    private class PickyCertificateProvider : ICertificateProvider
    {

        public PickyCertificateProvider(string host, X509Certificate2 certificate)
        {
            Host = host;
            Certificate = certificate;
        }

        private string Host { get; }

        private X509Certificate2 Certificate { get; }

        public X509Certificate2? Provide(string? host) => host == Host ? Certificate : null;
    }

}
