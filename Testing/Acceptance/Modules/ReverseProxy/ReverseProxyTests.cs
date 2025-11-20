using System.Linq;
using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.ReverseProxy;

using Cookie = GenHTTP.Api.Protocol.Cookie;

namespace GenHTTP.Testing.Acceptance.Modules.ReverseProxy;

[TestClass]
public sealed class ReverseProxyTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBasics(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r => r.Respond().Content("Hello World!").Build());

        var runner = setup.Runner;

        using var response = await runner.GetResponseAsync();
        Assert.AreEqual("Hello World!", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRedirection(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
        {
            return r.Respond().Header("Location", $"http://localhost:{r.EndPoint.Port}/target").Status(ResponseStatus.TemporaryRedirect).Build();
        });

        var runner = setup.Runner;

        using var redirected = await runner.GetResponseAsync("/");

        Assert.AreEqual($"http://localhost:{runner.Port}/target", redirected.GetHeader("Location"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestHead(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
        {
            var responseContent = new string('A', 2048);
            return r.Respond().Content(responseContent).Build();
        });

        var runner = setup.Runner;

        var headRequest = runner.GetRequest();
        headRequest.Method = HttpMethod.Head;

        using var headed = await runner.GetResponseAsync(headRequest);

        await headed.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCookies(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
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

        using var client = TestHost.GetClient(cookies: cookies);

        var cookieRequest = runner.GetRequest();

        using var cookied = await client.SendAsync(cookieRequest);

        await cookied.AssertStatusAsync(HttpStatusCode.OK);

        var returned = cookies.GetCookies(new Uri(runner.GetUrl()));

        Assert.AreEqual("1", returned["One"]!.Value);
        Assert.AreEqual("2", returned["Two"]!.Value);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestHeaders(TestEngine engine)
    {
        var now = DateTime.UtcNow;

        await using var setup = await TestSetup.CreateAsync(engine, r =>
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

        using var response = await runner.GetResponseAsync(request);

        Assert.AreEqual("Custom Value", response.GetHeader("X-Custom-Header"));

        Assert.AreEqual(now.ToString("r"), response.GetContentHeader("Expires"));
        Assert.AreEqual(now.ToString("r"), response.GetContentHeader("Last-Modified"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPost(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
        {
            using var reader = new StreamReader(r.Content!);
            return r.Respond().Content(reader.ReadToEnd()).Build();
        });

        var runner = setup.Runner;

        var request = runner.GetRequest();

        request.Method = HttpMethod.Post;
        request.Content = new StringContent("Input");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("Input", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPathing(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
        {
            return r.Respond().Content(r.Target.Path.ToString()).Build();
        });

        var runner = setup.Runner;

        using var r1 = await runner.GetResponseAsync("/");
        Assert.AreEqual("/", await r1.GetContentAsync());

        using var r2 = await runner.GetResponseAsync("/login/");
        Assert.AreEqual("/login/", await r2.GetContentAsync());

        using var r3 = await runner.GetResponseAsync("/login");
        Assert.AreEqual("/login", await r3.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestQuery(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
        {
            var result = string.Join('|', r.Query.Select(kv => $"{kv.Key}={kv.Value}"));
            return r.Respond().Content(result).Build();
        });

        var runner = setup.Runner;

        using var r2 = await runner.GetResponseAsync("/?one=two");
        Assert.AreEqual("one=two", await r2.GetContentAsync());

        using var r3 = await runner.GetResponseAsync("/?one=two&three=four");
        Assert.AreEqual("one=two|three=four", await r3.GetContentAsync());

        using var r1 = await runner.GetResponseAsync("/");
        Assert.AreEqual("", await r1.GetContentAsync());
    }

    [TestMethod]
    public async Task TestQuerySpecialChars()
    {
        await using var setup = await TestSetup.CreateAsync(TestEngine.Internal, r =>
        {
            var result = string.Join('|', r.Query.Select(kv => $"{kv.Key}={kv.Value}"));
            return r.Respond().Content(result).Build();
        });

        var runner = setup.Runner;

        using var r = await runner.GetResponseAsync("/?key=%20%3C+");
        Assert.AreEqual("key= <+", await r.GetContentAsync());
    }

    [TestMethod]
    public async Task TestPathSpecialChars()
    {
        await using var setup = await TestSetup.CreateAsync(TestEngine.Internal, r =>
        {
            return r.Respond().Content(r.Target.Path.ToString(true)).Build();
        });

        var runner = setup.Runner;

        using var r = await runner.GetResponseAsync("/%3F%23%26%2F %20");
        Assert.AreEqual("/%3F%23%26%2F%20%20", await r.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPathPreservesSpecialChars(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
        {
            return r.Respond().Content(r.Target.Path.ToString(true)).Build();
        });

        var runner = setup.Runner;

        using var r = await runner.GetResponseAsync("/$@:");
        Assert.AreEqual("/$@:", await r.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestContentLengthPreserved(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
        {
            return r.Respond()
                    .Content("Hello World")
                    .Type(new FlexibleContentType(ContentType.ImageJpg))
                    .Build();
        });

        using var response = await setup.Runner.GetResponseAsync();

        Assert.AreEqual(11, response.Content.Headers.ContentLength);
        AssertX.IsNullOrEmpty(response.GetHeader("Transfer-Encoding"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBadGateway(TestEngine engine)
    {
        var proxy = Proxy.Create()
                         .Upstream("http://icertainlydonotexistasadomain");

        await using var runner = await TestHost.RunAsync(proxy, engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.BadGateway);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCompression(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
        {
            var responseContent = new string('B', 2048);
            return r.Respond().Content(responseContent).Build();
        });

        var runner = setup.Runner;

        var request = runner.GetRequest();

        request.Headers.Add("Accept-Encoding", "br, gzip, deflate");

        using var response = await runner.GetResponseAsync(request);

        Assert.AreEqual("br", response.GetContentHeader("Content-Encoding"));
    }

    [TestMethod]
    public void TestAdjustments()
    {
        var i = 0;

        var proxy = Proxy.Create()
                         .Upstream("https://google.com")
                         .AdjustHandler(h => i++)
                         .AdjustClient(c => i++);

        proxy.Build();

        Assert.AreEqual(2, i);
    }

    #region Supporting data structures

    private class TestSetup : IAsyncDisposable
    {
        private readonly TestHost _target;

        private TestSetup(TestHost source, TestHost target)
        {
            Runner = source;
            _target = target;
        }

        public TestHost Runner { get; }

        public static async Task<TestSetup> CreateAsync(TestEngine engine, Func<IRequest, IResponse?> response)
        {
            // server hosting the actual web app
            var testServer = new TestHost(Layout.Create().Build(), false, engine: engine);

            await testServer.Host.Handler(new ProxiedRouter(response))
                      .StartAsync();

            // proxying server
            var proxy = Proxy.Create()
                             .ConnectTimeout(TimeSpan.FromSeconds(2))
                             .ReadTimeout(TimeSpan.FromSeconds(5))
                             .Upstream("http://localhost:" + testServer.Port);

            var runner = new TestHost(Layout.Create().Build(), engine: engine);

            await runner.Host.Handler(proxy)
                  .StartAsync();

            return new TestSetup(runner, testServer);
        }

        #region IDisposable Support

        private bool _disposedValue;

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    await Runner.DisposeAsync();
                    await _target.DisposeAsync();
                }

                _disposedValue = true;
            }
        }

        public ValueTask DisposeAsync() => DisposeAsync(true);

        #endregion

    }

    private class ProxiedRouter : IHandler
    {
        private readonly Func<IRequest, IResponse?> _response;

        public ProxiedRouter(Func<IRequest, IResponse?> response)
        {
            _response = response;
        }

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request) => new ProxiedProvider(_response).HandleAsync(request);

    }

    private class ProxiedProvider : IHandler
    {
        private readonly Func<IRequest, IResponse?> _response;

        public ProxiedProvider(Func<IRequest, IResponse?> response)
        {
            _response = response;
        }

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            Assert.AreNotEqual(request.Client, request.LocalClient);
            Assert.IsNotEmpty(request.Forwardings);

            var response = _response.Invoke(request);

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

}
