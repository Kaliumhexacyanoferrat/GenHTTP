using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Security.Providers;
using GenHTTP.Testing.Acceptance.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            await Assert.ThrowsExceptionAsync<HttpRequestException>(async () =>
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
            await Assert.ThrowsExceptionAsync<HttpRequestException>(async () =>
            {
                using var client = TestHost.GetClient(ignoreSecurityErrors: false);

                using var failedResponse = await client.GetAsync($"https://localhost:{sec}");
            });
        }, engine, host: "myserver");
    }

    private static async Task RunSecure(Func<ushort, ushort, Task> logic, TestEngine engine, SecureUpgrade? mode = null, string host = "localhost")
    {
        var content = Layout.Create().Index(Content.From(Resource.FromString("Hello Alice!")));

        await using var runner = new TestHost(Layout.Create().Build(), mode is null, engine: engine);

        var port = TestHost.NextPort();

        using var cert = await Security.GetCertificateAsync();

        runner.Host.Handler(content)
              .Bind(IPAddress.Any, (ushort)runner.Port)
              .Bind(IPAddress.Any, (ushort)port, new PickyCertificateProvider(host, cert), SslProtocols.Tls12);

        if (mode is not null)
        {
            runner.Host.SecureUpgrade(mode.Value);
            runner.Host.StrictTransport(new StrictTransportPolicy(TimeSpan.FromDays(365), true, true));
        }

        await runner.StartAsync();

        await logic((ushort)runner.Port, (ushort)port);
    }

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
