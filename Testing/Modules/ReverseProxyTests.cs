using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.ReverseProxy;
using GenHTTP.Modules.Layouting;

using Cookie = GenHTTP.Api.Protocol.Cookie;

namespace GenHTTP.Testing.Acceptance.Providers
{

    [TestClass]
    public sealed class ReverseProxyTests
    {

        #region Supporting data structures

        private class TestSetup : IDisposable
        {
            private readonly TestRunner _Target;

            public TestRunner Runner { get; }

            private TestSetup(TestRunner source, TestRunner target)
            {
                Runner = source;
                _Target = target;
            }

            public static TestSetup Create(Func<IRequest, IResponse?> response)
            {
                // server hosting the actuall web app
                var testServer = new TestRunner();

                testServer.Host.Handler(new ProxiedRouter(response).Wrap())
                               .Development()
                               .Start();

                // proxying server
                var proxy = Proxy.Create()
                                 .ConnectTimeout(TimeSpan.FromSeconds(2))
                                 .ReadTimeout(TimeSpan.FromSeconds(5))
                                 .Upstream("http://localhost:" + testServer.Port);

                var runner = new TestRunner();

                runner.Host.Handler(proxy)
                           .Development()
                           .Defaults()
                           .Start();

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
            private readonly Func<IRequest, IResponse?> _Response;

            public ProxiedRouter(Func<IRequest, IResponse?> response)
            {
                _Response = response;
            }

            public ValueTask PrepareAsync() => ValueTask.CompletedTask;

            public IHandler Parent => throw new NotImplementedException();

            public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => AsyncEnumerable.Empty<ContentElement>();

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                return new ProxiedProvider(_Response).HandleAsync(request);
            }

        }

        private class ProxiedProvider : IHandler
        {
            private readonly Func<IRequest, IResponse?> _Response;

            public ProxiedProvider(Func<IRequest, IResponse?> response)
            {
                _Response = response;
            }

            public IHandler Parent => throw new NotImplementedException();
            
            public ValueTask PrepareAsync() => ValueTask.CompletedTask;

            public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request)
            {
                throw new NotImplementedException();
            }

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                Assert.AreNotEqual(request.Client, request.LocalClient);
                Assert.IsTrue(request.Forwardings.Count > 0);

                var response = _Response.Invoke(request);

                if (response is not null)
                {
                    return new ValueTask<IResponse?>(response);
                }

                return request.Respond()
                              .Status(ResponseStatus.InternalServerError)
                              .BuildTask();
            }

        }

        #endregion

        [TestMethod]
        public async Task TestBasics()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Content("Hello World!").Build();
            });

            var runner = setup.Runner;

            using var response = await runner.GetResponse();
            Assert.AreEqual("Hello World!", await response.GetContent());
        }

        [TestMethod]
        public async Task TestRedirection()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Header("Location", $"http://localhost:{r.EndPoint.Port}/target").Status(ResponseStatus.TemporaryRedirect).Build();
            });

            var runner = setup.Runner;

            using var redirected = await runner.GetResponse("/");

            Assert.AreEqual($"http://localhost:{runner.Port}/target", redirected.GetHeader("Location"));
        }

        [TestMethod]
        public async Task TestHead()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Content("Hello World!").Build();
            });

            var runner = setup.Runner;

            var headRequest = runner.GetRequest();
            headRequest.Method = HttpMethod.Head;

            using var headed = await runner.GetResponse(headRequest);

            Assert.AreEqual(HttpStatusCode.OK, headed.StatusCode);
        }

        [TestMethod]
        public async Task TestCookies()
        {
            using var setup = TestSetup.Create((r) =>
            {
                Assert.AreEqual("World", r.Cookies["Hello"].Value);

                return r.Respond()
                        .Content("Hello World!")
                        .Cookie(new Cookie("One", "1"))
                        .Cookie(new Cookie("Two", "2"))
                        .Build();
            });

            var runner = setup.Runner;

            var cookies = new CookieContainer();
            cookies.Add(new System.Net.Cookie("Hello", "World", "/", "localhost"));

            using var client = TestRunner.GetClient(cookies: cookies);

            var cookieRequest = runner.GetRequest();

            using var cookied = await client.SendAsync(cookieRequest);

            Assert.AreEqual(HttpStatusCode.OK, cookied.StatusCode);

            var returned = cookies.GetCookies(new Uri(runner.GetUrl()));

            Assert.AreEqual("1", returned["One"]!.Value);
            Assert.AreEqual("2", returned["Two"]!.Value);
        }

        [TestMethod]
        public async Task TestHeaders()
        {
            var now = DateTime.UtcNow;

            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond()
                        .Content("Hello World")
                        .Header("X-Custom-Header", r.Headers["X-Custom-Header"])
                        .Expires(now)
                        .Modified(now)
                        .Build();
            });

            var runner = setup.Runner;

            var request = runner.GetRequest();

            request.Headers.Add("X-Custom-Header", "Custom Value");

            using var response = await runner.GetResponse(request);

            Assert.AreEqual("Custom Value", response.GetHeader("X-Custom-Header"));

            Assert.AreEqual(now.ToString("r"), response.GetContentHeader("Expires"));
            Assert.AreEqual(now.ToString("r"), response.GetContentHeader("Last-Modified"));
        }

        [TestMethod]
        public async Task TestPost()
        {
            using var setup = TestSetup.Create((r) =>
            {
                using var reader = new StreamReader(r.Content!);
                return r.Respond().Content(reader.ReadToEnd()).Build();
            });

            var runner = setup.Runner;

            var request = runner.GetRequest();

            request.Method = HttpMethod.Post;
            request.Content = new StringContent("Input");

            using var response = await runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Input", await response.GetContent());
        }

        [TestMethod]
        public async Task TestPathing()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Content(r.Target.Path.ToString()).Build();
            });

            var runner = setup.Runner;

            using var r1 = await runner.GetResponse("/");
            Assert.AreEqual("/", await r1.GetContent());

            using var r2 = await runner.GetResponse("/login/");
            Assert.AreEqual("/login/", await r2.GetContent());

            using var r3 = await runner.GetResponse("/login");
            Assert.AreEqual("/login", await r3.GetContent());
        }

        [TestMethod]
        public async Task TestQuery()
        {
            using var setup = TestSetup.Create((r) =>
            {
                var result = string.Join('|', r.Query.Select(kv => $"{kv.Key}={kv.Value}"));
                return r.Respond().Content(result).Build();
            });

            var runner = setup.Runner;

            using var r2 = await runner.GetResponse("/?one=two");
            Assert.AreEqual("one=two", await r2.GetContent());

            using var r3 = await runner.GetResponse("/?one=two&three=four");
            Assert.AreEqual("one=two|three=four", await r3.GetContent());

            using var r1 = await runner.GetResponse("/");
            Assert.AreEqual("", await r1.GetContent());
        }

        [TestMethod]
        public async Task TestQuerySpecialChars()
        {
            using var setup = TestSetup.Create((r) =>
            {
                var result = string.Join('|', r.Query.Select(kv => $"{kv.Key}={kv.Value}"));
                return r.Respond().Content(result).Build();
            });

            var runner = setup.Runner;

            using var r = await runner.GetResponse("/?key=%20%3C+");
            Assert.AreEqual("key= <+", await r.GetContent());
        }

        [TestMethod]
        public async Task TestPathSpecialChars()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Content(r.Target.Path.ToString(true)).Build();
            });

            var runner = setup.Runner;

            using var r = await runner.GetResponse("/%3F%23%26%2F %20");
            Assert.AreEqual("/%3F%23%26%2F%20%20", await r.GetContent());
        }

        [TestMethod]
        public async Task TestPathPreservesSpecialChars()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Content(r.Target.Path.ToString(true)).Build();
            });

            var runner = setup.Runner;

            using var r = await runner.GetResponse("/$@:");
            Assert.AreEqual("/$@:", await r.GetContent());
        }

        [TestMethod]
        public async Task TestContentLengthPreserved()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond()
                        .Content("Hello World")
                        .Type(new(ContentType.ImageJpg))
                        .Build();
            });

            using var response = await setup.Runner.GetResponse();

            Assert.AreEqual(11, response.Content.Headers.ContentLength);
            AssertX.IsNullOrEmpty(response.GetHeader("Transfer-Encoding"));
        }

        [TestMethod]
        public async Task TestBadGateway()
        {
            var proxy = Proxy.Create()
                             .Upstream("http://127.0.0.2");
            
            using var runner = TestRunner.Run(proxy);

            using var response = await runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.BadGateway, response.StatusCode);
        }


        [TestMethod]
        public async Task TestCompression()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Content("Hello World!").Build();
            });

            var runner = setup.Runner;

            var request = runner.GetRequest();

            request.Headers.Add("Accept-Encoding", "br, gzip, deflate");

            using var response = await runner.GetResponse(request);

            Assert.AreEqual("br", response.GetContentHeader("Content-Encoding"));
        }

    }

}
