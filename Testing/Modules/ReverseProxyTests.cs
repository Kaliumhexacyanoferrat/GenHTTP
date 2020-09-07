using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;

using Xunit;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.ReverseProxy;
using GenHTTP.Modules.Layouting;

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
                var proxy = Proxy.Create()
                                 .ConnectTimeout(TimeSpan.FromSeconds(2))
                                 .ReadTimeout(TimeSpan.FromSeconds(5))
                                 .Upstream("http://localhost:" + testServer.Port);

                var layout = Layout.Create().Fallback(proxy);

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

        private class ProxiedRouter : IHandler
        {
            private Func<IRequest, IResponseBuilder?> _Response;

            public ProxiedRouter(Func<IRequest, IResponseBuilder?> response)
            {
                _Response = response;
            }

            public IHandler Parent => throw new NotImplementedException();

            public IEnumerable<ContentElement> GetContent(IRequest request)
            {
                return new List<ContentElement>();
            }

            public IResponse? Handle(IRequest request)
            {
                return new ProxiedProvider(_Response).Handle(request);
            }

        }

        private class ProxiedProvider : IHandler
        {
            private Func<IRequest, IResponseBuilder?> _Response;

            public ProxiedProvider(Func<IRequest, IResponseBuilder?> response)
            {
                _Response = response;
            }

            public IHandler Parent => throw new NotImplementedException();

            public IEnumerable<ContentElement> GetContent(IRequest request)
            {
                throw new NotImplementedException();
            }

            public IResponse? Handle(IRequest request)
            {
                Assert.NotEqual(request.Client, request.LocalClient);
                Assert.NotEmpty(request.Forwardings);

                var response = _Response.Invoke(request);

                if (response != null)
                {
                    return response.Build();
                }

                return request.Respond()
                              .Status(ResponseStatus.InternalServerError)
                              .Build();
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
                return r.Respond().Header("Location", $"http://localhost:{r.EndPoint.Port}/target").Status(ResponseStatus.TemporaryRedirect);
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
                Assert.Equal("World", r.Cookies["Hello"].Value);

                return r.Respond().Content("Hello World!")
                                  .Cookie(new Cookie("One", "1"))
                                  .Cookie(new Cookie("Two", "2"));
            });

            var runner = setup.Runner;

            var cookieRequest = runner.GetRequest();
            cookieRequest.CookieContainer = new CookieContainer();
            cookieRequest.CookieContainer.Add(new System.Net.Cookie("Hello", "World", "/", "localhost"));

            using var cookied = cookieRequest.GetSafeResponse();

            Assert.Equal("1", cookied.Cookies["One"].Value);
            Assert.Equal("2", cookied.Cookies["Two"].Value);
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
                using var reader = new StreamReader(r.Content!);
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

            using var response = request.GetSafeResponse();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Input", response.GetContent());
        }

        [Fact]
        public void TestPathing()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Content(r.Target.Path.ToString());
            });

            var runner = setup.Runner;

            using var r1 = runner.GetResponse("/");
            Assert.Equal("/", r1.GetContent());

            using var r2 = runner.GetResponse("/login/");
            Assert.Equal("/login/", r2.GetContent());

            using var r3 = runner.GetResponse("/login");
            Assert.Equal("/login", r3.GetContent());
        }

        [Fact]
        public void TestQuery()
        {
            using var setup = TestSetup.Create((r) =>
            {
                var result = string.Join('|', r.Query.Select(kv => $"{kv.Key}={kv.Value}"));
                return r.Respond().Content(result);
            });

            var runner = setup.Runner;

            using var r2 = runner.GetResponse("/?one=two");
            Assert.Equal("one=two", r2.GetContent());

            using var r3 = runner.GetResponse("/?one=two&three=four");
            Assert.Equal("one=two|three=four", r3.GetContent());

            using var r1 = runner.GetResponse("/");
            Assert.Equal("", r1.GetContent());
        }

        [Fact]
        public void TestQuerySpecialChars()
        {
            using var setup = TestSetup.Create((r) =>
            {
                var result = string.Join('|', r.Query.Select(kv => $"{kv.Key}={kv.Value}"));
                return r.Respond().Content(result);
            });

            var runner = setup.Runner;

            using var r = runner.GetResponse("/?key=%20%3C+");
            Assert.Equal("key= <+", r.GetContent());
        }

        [Fact]
        public void TestBadGateway()
        {
            var proxy = Proxy.Create()
                             .Upstream("http://127.0.0.2");

            var layout = Layout.Create().Fallback(proxy);

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        }

    }

}
