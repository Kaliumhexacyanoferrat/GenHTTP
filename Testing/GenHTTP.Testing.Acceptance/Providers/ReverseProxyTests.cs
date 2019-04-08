using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

using Xunit;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core;
using GenHTTP.Testing.Acceptance.Domain;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class ReverseProxyTests
    {

        private class ProxiedProvider : IContentProvider
        {

            public IResponseBuilder Handle(IRequest request)
            {
                var content = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!"));
                return request.Respond().Content(content, ContentType.TextPlain);
            }

        }

        /// <summary>
        /// As a developer, I would like to serve content from
        /// another web server via a reverse proxy.
        /// </summary>
        [Fact]
        public void TestRegular()
        {
            // server hosting the actuall web app
            var testApp = Layout.Create().Add("_", new ProxiedProvider(), true);

            using var testServer = TestRunner.Run(testApp);

            // proxying server
            var proxy = ReverseProxy.Create()
                                    .Upstream("http://localhost:" + testServer.Port);

            var layout = Layout.Create().Add("_", proxy, true);

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse();

            Assert.Equal("Hello World!", response.GetContent());
        }

        /// <summary>
        /// As a developer, I excpect the proxy to return correct status codes.
        /// </summary>
        [Fact]
        public void TestBadGateway()
        {
            var proxy = ReverseProxy.Create()
                                    .Upstream("http://127.0.0.2");

            var layout = Layout.Create().Add("_", proxy, true);

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        }

    }

}
