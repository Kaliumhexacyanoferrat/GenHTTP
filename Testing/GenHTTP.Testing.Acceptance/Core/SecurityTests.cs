using System;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;

using Xunit;

using GenHTTP.Testing.Acceptance.Domain;
using GenHTTP.Modules.Core;
using GenHTTP.Api.Infrastructure;

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
            }, SecureUpgrade.Allow);
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
            }, SecureUpgrade.Allow);
        }

        private static void RunSecure(Action<ushort, ushort> logic, SecureUpgrade? mode = null)
        {
            var content = Layout.Create().Add("index", Content.From("Hello Alice!"), true);

            using var runner = new TestRunner();

            var port = TestRunner.NextPort();

            using var cert = GetCertificate();

            var builder = runner.Builder
                                .Router(content)
                                .Bind(IPAddress.Any, runner.Port)
                                .Bind(IPAddress.Any, port, "localhost", cert);

            if (mode != null)
            {
                builder.Security(mode.Value);
            }

            using var _ = builder.Build();

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

    }
    
}

