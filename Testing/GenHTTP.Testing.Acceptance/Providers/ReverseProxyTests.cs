using System;
using System.IO;
using System.Net;
using System.Text;

using Xunit;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core;
using GenHTTP.Testing.Acceptance.Domain;

using Cookie = GenHTTP.Api.Protocol.Cookie;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class ReverseProxyTests
    {

        private class ProxiedProvider : IContentProvider
        {

            public IResponseBuilder Handle(IRequest request)
            {
                if (request.Query.ContainsKey("location"))
                {
                    return request.Respond().Header("Location", $"http://localhost:{request.EndPoint.Port}/target");
                }

                var content = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!"));

                var response = request.Respond().Content(content, ContentType.TextPlain);

                if (request.Cookies.Count > 0)
                {
                    response.Cookie(new Cookie("Bla", "Blubb"));
                }

                return response;
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
                                    .ConnectTimeout(TimeSpan.FromSeconds(2))
                                    .ReadTimeout(TimeSpan.FromSeconds(5))
                                    .Upstream("http://localhost:" + testServer.Port);

            var layout = Layout.Create().Add("_", proxy, true);

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse();

            Assert.Equal("Hello World!", response.GetContent());

            // test redirection
            using var redirected = runner.GetResponse("/?location=1");

            Assert.Equal(redirected.Headers["Location"], $"http://localhost:{runner.Port}/target");

            // test cookies
            var cookieRequest = runner.GetRequest();
            cookieRequest.CookieContainer = new CookieContainer();
            cookieRequest.CookieContainer.Add(new System.Net.Cookie("Hello", "World", "/", "localhost"));

            using var cookied = cookieRequest.GetSafeResponse();

            Assert.True(cookied.Cookies.Count > 0);
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
