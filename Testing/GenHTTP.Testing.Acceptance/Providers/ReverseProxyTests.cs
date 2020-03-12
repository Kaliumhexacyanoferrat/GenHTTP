using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text;

using Xunit;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.General;
using GenHTTP.Testing.Acceptance.Domain;

using Cookie = GenHTTP.Api.Protocol.Cookie;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class ReverseProxyTests
    {

        #region Supporting data structures

        private class TestSetup : IDisposable
        {
            private TestRunner _Target;

            public TestRunner Runner { get; }

            private TestSetup(TestRunner source, TestRunner target)
            {
                Runner = source;
                _Target = target;
            }

            public static TestSetup Create(Func<IRequest, IResponseBuilder?> response)
            {
                // server hosting the actuall web app
                var testServer = TestRunner.Run(new ProxiedRouter(response));

                // proxying server
                var proxy = ReverseProxy.Create()
                                        .ConnectTimeout(TimeSpan.FromSeconds(2))
                                        .ReadTimeout(TimeSpan.FromSeconds(5))
                                        .Upstream("http://localhost:" + testServer.Port);

                var layout = Layout.Create().Add("_", proxy, true);

                var runner = TestRunner.Run(layout);

                return new TestSetup(runner, testServer);
            }

            #region IDisposable Support

            private bool disposedValue = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        Runner.Dispose();
                        _Target.Dispose();
                    }

                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }

            #endregion

        }

        private class ProxiedRouter : RouterBase
        {
            private Func<IRequest, IResponseBuilder?> _Response;

            public ProxiedRouter(Func<IRequest, IResponseBuilder?> response) : base(null, null)
            {
                _Response = response;
            }

            public override IEnumerable<ContentElement> GetContent(IRequest request, string basePath)
            {
                return new List<ContentElement>();
            }

            public override void HandleContext(IEditableRoutingContext current)
            {
                current.RegisterContent(new ProxiedProvider(_Response));
            }

            public override string? Route(string path, int currentDepth)
            {
                return null;
            }

        }

        private class ProxiedProvider : IContentProvider
        {
            private Func<IRequest, IResponseBuilder?> _Response;

            public FlexibleContentType? ContentType => new FlexibleContentType(Api.Protocol.ContentType.TextPlain);

            public string? Title => null;

            public ProxiedProvider(Func<IRequest, IResponseBuilder?> response)
            {
                _Response = response;
            }

            public IResponseBuilder Handle(IRequest request)
            {
                Assert.NotEqual(request.Client, request.LocalClient);
                Assert.NotEmpty(request.Forwardings);

                var response = _Response.Invoke(request);

                if (response != null)
                {
                    return response;
                }

                return request.Respond(ResponseStatus.InternalServerError);
            }

        }

        #endregion

        [Fact]
        public void TestBasics()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Content("Hello World!");
            });

            var runner = setup.Runner;

            using var response = runner.GetResponse();
            Assert.Equal("Hello World!", response.GetContent());
        }

        [Fact]
        public void TestRedirection()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Header("Location", $"http://localhost:{r.EndPoint.Port}/target");
            });

            var runner = setup.Runner;

            using var redirected = runner.GetResponse("/");

            Assert.Equal(redirected.Headers["Location"], $"http://localhost:{runner.Port}/target");
        }

        [Fact]
        public void TestHead()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Content("Hello World!");
            });

            var runner = setup.Runner;

            var headRequest = runner.GetRequest();
            headRequest.Method = "HEAD";

            using var headed = headRequest.GetSafeResponse();

            Assert.Equal(HttpStatusCode.OK, headed.StatusCode);
        }

        [Fact]
        public void TestCookies()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Content("Hello World!").Cookie(new Cookie("Bla", "Blubb"));
            });

            var runner = setup.Runner;

            var cookieRequest = runner.GetRequest();
            cookieRequest.CookieContainer = new CookieContainer();
            cookieRequest.CookieContainer.Add(new System.Net.Cookie("Hello", "World", "/", "localhost"));

            using var cookied = cookieRequest.GetSafeResponse();

            Assert.True(cookied.Cookies.Count > 0);
        }

        [Fact]
        public void TestHeaders()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond()
                        .Content("Hello World")
                        .Header("X-Custom-Header", r.Headers["X-Custom-Header"]);
            });

            var runner = setup.Runner;

            var request = runner.GetRequest();

            request.Headers.Add("X-Custom-Header", "Custom Value");

            using var response = request.GetResponse();

            Assert.Equal("Custom Value", response.Headers.Get("X-Custom-Header"));
        }

        [Fact]
        public void TestPost()
        {
            using var setup = TestSetup.Create((r) =>
            {
                var reader = new StreamReader(r.Content!);
                return r.Respond().Content(reader.ReadToEnd());
            });

            var runner = setup.Runner;

            var request = runner.GetRequest();

            request.Method = "POST";
            request.ContentType = "text/plain";

            using (var stream = request.GetRequestStream())
            {
                var input = Encoding.UTF8.GetBytes("Input");
                stream.Write(input, 0, input.Length);
            }

            Assert.Equal("Input", request.GetSafeResponse().GetContent());
        }

        [Fact]
        public void TestPathing()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Content(r.Path);
            });

            var runner = setup.Runner;

            using var r1 = runner.GetResponse("/");
            Assert.Equal("/", r1.GetContent());

            using var r2 = runner.GetResponse("/login/");
            Assert.Equal("/login/", r2.GetContent());

            using var r3 = runner.GetResponse("/login");
            Assert.Equal("/login", r3.GetContent());
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
