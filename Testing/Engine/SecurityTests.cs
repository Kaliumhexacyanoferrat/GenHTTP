using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Security;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Security.Providers;
using System.Threading.Tasks;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public class SecurityTests
    {

        /// <summary>
        /// As a developer I would like to serve my application in a secure manner.
        /// </summary>
        [TestMethod]
        public Task TestSecure()
        {
            return RunSecure((insec, sec) =>
            {
                var request = WebRequest.CreateHttp($"https://localhost:{sec}");
                request.IgnoreSecurityErrors();

                using var response = request.GetSafeResponse();

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual("Hello Alice!", response.GetContent());
            });
        }

        /// <summary>
        /// As a developer, I expect the server to redirect to a secure endpoint
        /// by default.
        /// </summary>
        [TestMethod]
        public Task TestDefaultRedirection()
        {
            return RunSecure((insec, sec) =>
            {
                var request = WebRequest.CreateHttp($"http://localhost:{insec}");
                request.AllowAutoRedirect = false;

                using var response = request.GetSafeResponse();

                Assert.AreEqual(HttpStatusCode.MovedPermanently, response.StatusCode);
                Assert.AreEqual($"https://localhost:{sec}/", response.Headers["Location"]);
            });
        }

        /// <summary>
        /// As a developer, I expect HTTP requests not to be redirected if
        /// upgrades are allowed but not requested by the client.
        /// </summary>
        [TestMethod]
        public Task TestNoRedirectionWithAllowed()
        {
            return RunSecure((insec, sec) =>
            {
                var request = WebRequest.CreateHttp($"http://localhost:{insec}");
                request.AllowAutoRedirect = false;

                using var response = request.GetSafeResponse();

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }, mode: SecureUpgrade.Allow);
        }

        /// <summary>
        /// As I developer, I expect requests to be upgraded if requested
        /// by the client.
        /// </summary>
        [TestMethod]
        public Task TestRedirectionWhenRequested()
        {
            return RunSecure((insec, sec) =>
            {
                var request = WebRequest.CreateHttp($"http://localhost:{insec}");
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.AllowAutoRedirect = false;

                using var response = request.GetSafeResponse();

                Assert.AreEqual(HttpStatusCode.TemporaryRedirect, response.StatusCode);
                Assert.AreEqual($"https://localhost:{sec}/", response.Headers["Location"]);
                Assert.AreEqual($"Upgrade-Insecure-Requests", response.Headers["Vary"]);
            }, mode: SecureUpgrade.Allow);
        }

        /// <summary>
        /// As the hoster of a web application, I want my application to enforce strict
        /// transport security, so that man-in-the-middle attacks can be avoided to some extend.
        /// </summary>
        [TestMethod]
        public Task TestTransportPolicy()
        {
            return RunSecure((insec, sec) =>
            {
                var insecureRequest = WebRequest.CreateHttp($"http://localhost:{insec}");

                using var insecureResponse = insecureRequest.GetSafeResponse();

                Assert.AreEqual(HttpStatusCode.OK, insecureResponse.StatusCode);
                Assert.IsNull(insecureResponse.Headers["Strict-Transport-Security"]);

                var secureRequest = WebRequest.CreateHttp($"https://localhost:{sec}");
                secureRequest.IgnoreSecurityErrors();

                using var secureResponse = secureRequest.GetSafeResponse();

                Assert.AreEqual(HttpStatusCode.OK, secureResponse.StatusCode);
                Assert.AreEqual("max-age=31536000; includeSubDomains; preload", secureResponse.Headers["Strict-Transport-Security"]);

            }, mode: SecureUpgrade.None);
        }

        /// <summary>
        /// As the operator of the server, I expect the server to resume
        /// normal operation after a security error has happened.
        /// </summary>
        [TestMethod]
        public Task TestSecurityError()
        {
            return RunSecure((insec, sec) =>
            {
                Assert.ThrowsException<WebException>(() =>
                {
                    var failedRequest = WebRequest.CreateHttp($"https://localhost:{sec}");
                    failedRequest.GetSafeResponse();
                });

                var okayRequest = WebRequest.CreateHttp($"https://localhost:{sec}");
                okayRequest.IgnoreSecurityErrors();

                using var response = okayRequest.GetSafeResponse();

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            });
        }

        /// <summary>
        /// As a web developer, I can decide not to return a certificate which will
        /// abort the server SSL handshake.
        /// </summary>
        [TestMethod]
        public Task TestNoCertificate()
        {
            return RunSecure((insec, sec) =>
            {
                Assert.ThrowsException<WebException>(() =>
                {
                    var failedRequest = WebRequest.CreateHttp($"https://localhost:{sec}");
                    failedRequest.IgnoreSecurityErrors();

                    failedRequest.GetSafeResponse();
                });
            }, host: "myserver");
        }

        private static async Task RunSecure(Action<ushort, ushort> logic, SecureUpgrade? mode = null, string host = "localhost")
        {
            var content = Layout.Create().Index(Content.From(Resource.FromString("Hello Alice!")));

            using var runner = new TestRunner(mode is null);

            var port = TestRunner.NextPort();

            using var cert = await GetCertificate();

            runner.Host.Handler(content)
                       .Bind(IPAddress.Any, (ushort)runner.Port)
                       .Bind(IPAddress.Any, (ushort)port, new PickyCertificateProvider(host, cert), SslProtocols.Tls12);

            if (mode is not null)
            {
                runner.Host.SecureUpgrade(mode.Value);
                runner.Host.StrictTransport(new StrictTransportPolicy(TimeSpan.FromDays(365), true, true));
            }

            runner.Start();

            logic((ushort)runner.Port, (ushort)port);
        }

        private static async ValueTask<X509Certificate2> GetCertificate()
        {
            using (var stream = await Resource.FromAssembly("Certificate.pfx").Build().GetContentAsync())
            {
                using (var mem = new MemoryStream())
                {
                    await stream.CopyToAsync(mem);
                    return new X509Certificate2(mem.ToArray());
                }
            }
        }

        private class PickyCertificateProvider : ICertificateProvider
        {

            private string Host { get; }

            private X509Certificate2 Certificate { get; }

            public PickyCertificateProvider(string host, X509Certificate2 certificate)
            {
                Host = host;
                Certificate = certificate;
            }

            public X509Certificate2? Provide(string? host)
            {
                return host == Host ? Certificate : null;
            }

        }

    }

}

