using System;
using System.Net;
using System.Text;

using Xunit;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.General;
using GenHTTP.Testing.Acceptance.Domain;

using Cookie = GenHTTP.Api.Protocol.Cookie;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class ReverseProxyTests
    {

        private class ProxiedProvider : IContentProvider
        {

            public FlexibleContentType? ContentType => new FlexibleContentType(Api.Protocol.ContentType.TextPlain);

            public string? Title => null;

            public IResponseBuilder Handle(IRequest request)
            {
                if (request.Query.ContainsKey("location"))
                {
                    return request.Respond().Header("Location", $"http://localhost:{request.EndPoint.Port}/target");
                }
                
                var response = request.Respond()
                                      .Content(new StringContent("Hello World!"))
                                      .Type(ContentType!);

                if (request.Cookies.Count > 0)
                {
                    response.Cookie(new Cookie("Bla", "Blubb"));
                }

                if (request.Headers.ContainsKey("X-Custom-Header"))
                {
                    response.Header("X-Custom-Header", request.Headers["X-Custom-Header"]);
                }

                Assert.NotEqual(request.Client, request.LocalClient);
                Assert.NotEmpty(request.Forwardings);
                
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

            var request = runner.GetRequest();

            request.Method = "POST";
            request.Headers.Add("X-Custom-Header", "Custom Value");
            request.ContentType = "text/plain";

            using (var stream = request.GetRequestStream())
            {
                var input = Encoding.UTF8.GetBytes("Input");
                stream.Write(input, 0, input.Length);
            }

            var response = request.GetSafeResponse();

            Assert.Equal("Hello World!", response.GetContent());
            Assert.Equal("Custom Value", response.Headers.Get("X-Custom-Header"));

            // test redirection
            using var redirected = runner.GetResponse("/?location=1");

            Assert.Equal(redirected.Headers["Location"], $"http://localhost:{runner.Port}/target");

            // test head
            var headRequest = runner.GetRequest();
            headRequest.Method = "HEAD";

            using var headed = headRequest.GetSafeResponse();

            Assert.Equal(HttpStatusCode.OK, headed.StatusCode);

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
