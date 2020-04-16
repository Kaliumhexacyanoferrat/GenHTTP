using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using Xunit;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.Security;
using GenHTTP.Testing.Acceptance.Domain;

namespace GenHTTP.Testing.Acceptance.Core
{

    public class SecurityTests
    {

        /// <summary>
        /// As a developer I would like to serve my application in a secure manner.
        /// </summary>
        [Fact]
        public void TestSecure()
        {
            RunSecure((insec, sec) =>
            {
                var request = WebRequest.CreateHttp($"https://localhost:{sec}");
                request.IgnoreSecurityErrors();

                using var response = request.GetSafeResponse();

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Hello Alice!", response.GetContent());
            });
        }

        /// <summary>
        /// As a developer, I expect the server to redirect to a secure endpoint
        /// by default.
        /// </summary>
        [Fact]
        public void TestDefaultRedirection()
        {
            RunSecure((insec, sec) =>
            {
                var request = WebRequest.CreateHttp($"http://localhost:{insec}");
                request.AllowAutoRedirect = false;

                using var response = request.GetSafeResponse();

                Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
                Assert.Equal($"https://localhost:{sec}/", response.Headers["Location"]);
            });
        }

        /// <summary>
        /// As a developer, I expect HTTP requests not to be redirected if
        /// upgrades are allowed but not requested by the client.
        /// </summary>
        [Fact]
        public void TestNoRedirectionWithAllowed()
        {
            RunSecure((insec, sec) =>
            {
                var request = WebRequest.CreateHttp($"http://localhost:{insec}");
                request.AllowAutoRedirect = false;

                using var response = request.GetSafeResponse();

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }, mode: SecureUpgrade.Allow);
        }

        /// <summary>
        /// As I developer, I expect requests to be upgraded if requested
        /// by the client.
        /// </summary>
        [Fact]
        public void TestRedirectionWhenRequested()
        {
            RunSecure((insec, sec) =>
            {
                var request = WebRequest.CreateHttp($"http://localhost:{insec}");
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.AllowAutoRedirect = false;

                using var response = request.GetSafeResponse();

                Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
                Assert.Equal($"https://localhost:{sec}/", response.Headers["Location"]);
                Assert.Equal($"Upgrade-Insecure-Requests", response.Headers["Vary"]);
            }, mode: SecureUpgrade.Allow);
        }

        /// <summary>
        /// As the hoster of a web application, I want my application to enforce strict
        /// transport security, so that man-in-the-middle attacks can be avoided to some extend.
        /// </summary>
        [Fact]
        public void TestTransportPolicy()
        {
            RunSecure((insec, sec) =>
            {
                var insecureRequest = WebRequest.CreateHttp($"http://localhost:{insec}");

                using var insecureResponse = insecureRequest.GetSafeResponse();

                Assert.Equal(HttpStatusCode.OK, insecureResponse.StatusCode);
                Assert.Null(insecureResponse.Headers["Strict-Transport-Security"]);

                var secureRequest = WebRequest.CreateHttp($"https://localhost:{sec}");
                secureRequest.IgnoreSecurityErrors();

                using var secureResponse = secureRequest.GetSafeResponse();

                Assert.Equal(HttpStatusCode.OK, secureResponse.StatusCode);
                Assert.Equal("max-age=31536000; includeSubDomains; preload", secureResponse.Headers["Strict-Transport-Security"]);

            }, mode: SecureUpgrade.None);
        }

        /// <summary>
        /// As the operator of the server, I expect the server to resume
        /// normal operation after a security error has happened.
        /// </summary>
        [Fact]
        public void TestSecurityError()
        {
            RunSecure((insec, sec) =>
            {
                Assert.Throws<WebException>(() =>
                {
                    var failedRequest = WebRequest.CreateHttp($"https://localhost:{sec}");
                    failedRequest.GetSafeResponse();
                });

                var okayRequest = WebRequest.CreateHttp($"https://localhost:{sec}");
                okayRequest.IgnoreSecurityErrors();

                using var response = okayRequest.GetSafeResponse();

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        /// <summary>
        /// As a web developer, I can decide not to return a certificate which will
        /// abort the server SSL handshake.
        /// </summary>
        [Fact]
        public void TestNoCertificate()
        {
            RunSecure((insec, sec) =>
            {
                Assert.Throws<WebException>(() =>
                {
                    var failedRequest = WebRequest.CreateHttp($"https://localhost:{sec}");
                    failedRequest.IgnoreSecurityErrors();

                    failedRequest.GetSafeResponse();
                });
            }, host: "myserver");
        }

        private static void RunSecure(Action<ushort, ushort> logic, SecureUpgrade? mode = null, string host = "localhost")
        {
            var content = Layout.Create().Index(Content.From("Hello Alice!"));

            using var runner = new TestRunner(mode == null);

            var port = TestRunner.NextPort();

            using var cert = GetCertificate();

            runner.Host.Handler(content)
                       .Bind(IPAddress.Any, runner.Port)
                       .Bind(IPAddress.Any, port, new PickyCertificateProvider(host, cert), SslProtocols.Tls12);

            if (mode != null)
            {
                runner.Host.SecureUpgrade(mode.Value);
                runner.Host.StrictTransport(new StrictTransportPolicy(TimeSpan.FromDays(365), true, true));
            }

            runner.Start();

            logic(runner.Port, port);
        }

        private static X509Certificate2 GetCertificate()
        {
            using (var stream = Data.FromResource("Certificate.pfx").Build().GetResource())
            {
                using (var mem = new MemoryStream())
                {
                    stream.CopyTo(mem);
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
                return (host == Host) ? Certificate : null;
            }

        }

    }

}

