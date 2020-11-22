using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
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
            private TestRunner _Target;

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
            private Func<IRequest, IResponse?> _Response;

            public ProxiedRouter(Func<IRequest, IResponse?> response)
            {
                _Response = response;
            }

            public IHandler Parent => throw new NotImplementedException();

            public IEnumerable<ContentElement> GetContent(IRequest request)
            {
                return new List<ContentElement>();
            }

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                return new ProxiedProvider(_Response).HandleAsync(request);
            }

        }

        private class ProxiedProvider : IHandler
        {
            private Func<IRequest, IResponse?> _Response;

            public ProxiedProvider(Func<IRequest, IResponse?> response)
            {
                _Response = response;
            }

            public IHandler Parent => throw new NotImplementedException();

            public IEnumerable<ContentElement> GetContent(IRequest request)
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
        public void TestBasics()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Content("Hello World!").Build();
            });

            var runner = setup.Runner;

            using var response = runner.GetResponse();
            Assert.AreEqual("Hello World!", response.GetContent());
        }

        [TestMethod]
        public void TestRedirection()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Header("Location", $"http://localhost:{r.EndPoint.Port}/target").Status(ResponseStatus.TemporaryRedirect).Build();
            });

            var runner = setup.Runner;

            using var redirected = runner.GetResponse("/");

            Assert.AreEqual($"http://localhost:{runner.Port}/target", redirected.Headers["Location"]);
        }

        [TestMethod]
        public void TestHead()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Content("Hello World!").Build();
            });

            var runner = setup.Runner;

            var headRequest = runner.GetRequest();
            headRequest.Method = "HEAD";

            using var headed = headRequest.GetSafeResponse();

            Assert.AreEqual(HttpStatusCode.OK, headed.StatusCode);
        }

        [TestMethod]
        public void TestCookies()
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

            var cookieRequest = runner.GetRequest();
            cookieRequest.CookieContainer = new CookieContainer();
            cookieRequest.CookieContainer.Add(new System.Net.Cookie("Hello", "World", "/", "localhost"));

            using var cookied = cookieRequest.GetSafeResponse();

            Assert.AreEqual("1", cookied.Cookies["One"]!.Value);
            Assert.AreEqual("2", cookied.Cookies["Two"]!.Value);
        }

        [TestMethod]
        public void TestHeaders()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond()
                        .Content("Hello World")
                        .Header("X-Custom-Header", r.Headers["X-Custom-Header"])
                        .Build();
            });

            var runner = setup.Runner;

            var request = runner.GetRequest();

            request.Headers.Add("X-Custom-Header", "Custom Value");

            using var response = request.GetResponse();

            Assert.AreEqual("Custom Value", response.Headers.Get("X-Custom-Header"));
        }

        [TestMethod]
        public void TestPost()
        {
            using var setup = TestSetup.Create((r) =>
            {
                using var reader = new StreamReader(r.Content!);
                return r.Respond().Content(reader.ReadToEnd()).Build();
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

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Input", response.GetContent());
        }

        [TestMethod]
        public void TestPathing()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Content(r.Target.Path.ToString()).Build();
            });

            var runner = setup.Runner;

            using var r1 = runner.GetResponse("/");
            Assert.AreEqual("/", r1.GetContent());

            using var r2 = runner.GetResponse("/login/");
            Assert.AreEqual("/login/", r2.GetContent());

            using var r3 = runner.GetResponse("/login");
            Assert.AreEqual("/login", r3.GetContent());
        }

        [TestMethod]
        public void TestQuery()
        {
            using var setup = TestSetup.Create((r) =>
            {
                var result = string.Join('|', r.Query.Select(kv => $"{kv.Key}={kv.Value}"));
                return r.Respond().Content(result).Build();
            });

            var runner = setup.Runner;

            using var r2 = runner.GetResponse("/?one=two");
            Assert.AreEqual("one=two", r2.GetContent());

            using var r3 = runner.GetResponse("/?one=two&three=four");
            Assert.AreEqual("one=two|three=four", r3.GetContent());

            using var r1 = runner.GetResponse("/");
            Assert.AreEqual("", r1.GetContent());
        }

        [TestMethod]
        public void TestQuerySpecialChars()
        {
            using var setup = TestSetup.Create((r) =>
            {
                var result = string.Join('|', r.Query.Select(kv => $"{kv.Key}={kv.Value}"));
                return r.Respond().Content(result).Build();
            });

            var runner = setup.Runner;

            using var r = runner.GetResponse("/?key=%20%3C+");
            Assert.AreEqual("key= <+", r.GetContent());
        }

        [TestMethod]
        public void TestPathSpecialChars()
        {
            using var setup = TestSetup.Create((r) =>
            {
                return r.Respond().Content(r.Target.Path.ToString(true)).Build();
            });

            var runner = setup.Runner;

            using var r = runner.GetResponse("/%3F%23%26%2F %20");
            Assert.AreEqual("/%3F%23%26%2F%20%20", r.GetContent());
        }

        [TestMethod]
        public void TestBadGateway()
        {
            var proxy = Proxy.Create()
                             .Upstream("http://127.0.0.2");

            var layout = Layout.Create().Fallback(proxy);

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.BadGateway, response.StatusCode);
        }

    }

}
