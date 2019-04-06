using System;
using System.Net;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Text;

using Xunit;

using GenHTTP.Testing.Acceptance.Domain;
using GenHTTP.Modules.Core;

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
            var content = Layout.Create().Add("index", Content.From("Hello Alice!"), true);

            using var runner = new TestRunner();

            var port = TestRunner.NextPort();

            using var cert = GetCertificate();

            using var _ = runner.Builder
                                .Router(content)
                                .Bind(IPAddress.Any, port, cert)
                                .Build();

            var request = WebRequest.CreateHttp($"https://localhost:{port}");

            request.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
            {
                return true;
            };

            using var response = request.GetSafeResponse();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Hello Alice!", response.GetContent());
        }

        private static X509Certificate GetCertificate()
        {
            using (var stream = Data.FromResource("Certificate.pfx").Build().GetResource())
            {
                using (var mem = new MemoryStream())
                {
                    stream.CopyTo(mem);
                    return new X509Certificate(mem.ToArray());
                }
            }
        }

    }
    
}

