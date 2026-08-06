using System.Net;
using System.Web;
using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.ReverseProxy;

using GenHTTP.Testing.Acceptance.Utilities;

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
    public async Task TestForwardingChainIsRelayed(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
        {
            var forwardings = r.Header.Headers.GetForwardings();

            // the proxy relays the original hop and appends one of its own
            Assert.AreEqual(2, forwardings.Count);
            Assert.AreEqual(IPAddress.Parse("85.192.1.5"), forwardings[0].For);
            Assert.AreEqual("google.com", forwardings[0].Host);

            Assert.IsTrue(IPAddress.IsLoopback(forwardings[1].For!));

            return r.Respond().Content("Hello World!").Build();
        });

        var runner = setup.Runner;

        var request = runner.GetRequest();
        request.Headers.Add("Forwarded", "for=85.192.1.5; host=google.com");

        using var response = await runner.GetResponseAsync(request);
        Assert.AreEqual("Hello World!", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCookies(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
        {
            Assert.AreEqual("World", r.Header.Headers.GetCookie("Hello"));

            return r.Respond()
                    .Content("Hello World!")
                    .Cookie("One", "1")
                    .Cookie("Two", "2")
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
                    .Header("X-Custom-Header", r.Header.Headers.GetEntry("X-Custom-Header") ?? "none")
                    .Build();
        });

        var runner = setup.Runner;

        var request = runner.GetRequest();

        request.Headers.Add("X-Custom-Header", "Custom Value");

        using var response = await runner.GetResponseAsync(request);

        Assert.AreEqual("Custom Value", response.GetHeader("X-Custom-Header"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPost(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
        {
            using var reader = new StreamReader(r.GetBody(HeaderAccess.Retain)!.AsStream());
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
            return r.Respond().Content(r.Header.Target.AsString(false)).Build();
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
            var entries = new List<string>();

            for (var i = 0; i < r.Header.Query.Count; i++)
            {
                var entry = r.Header.Query.GetStringEntry(i);

                var key = entry.Key.ToString();
                var value = entry.Value.ToString();

                entries.Add($"{key}={value}");
            }

            var result = string.Join('|', entries);
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
    [MultiEngineTest]
    public async Task TestQuerySpecialChars(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
        {
            var entries = new List<string>();

            for (var i = 0; i < r.Header.Query.Count; i++)
            {
                var entry = r.Header.Query.GetStringEntry(i);

                var key = entry.Key.ToString();
                var value = HttpUtility.UrlDecode(entry.Value.ToString());

                entries.Add($"{key}={value}");
            }

            var result = string.Join('|', entries);

            return r.Respond().Content(result).Build();
        });

        var runner = setup.Runner;

        using var r = await runner.GetResponseAsync("/?key=%20%3C+");
        Assert.AreEqual("key= < ", await r.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPathSpecialChars(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
        {
            return r.Respond().Content(r.Header.Target.AsString(decode: false)).Build();
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
            return r.Respond().Content(r.Header.Target.AsString()).Build();
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
                    .Content("Hello World", ContentType.ImageJpg)
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
    [MultiEngineTest]
    public async Task TestRedirectionToExternalHostIsNotRewritten(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
        {
            return r.Respond().Header("Location", "https://example.com/elsewhere").Status(ResponseStatus.TemporaryRedirect).Build();
        });

        using var redirected = await setup.Runner.GetResponseAsync("/");

        Assert.AreEqual("https://example.com/elsewhere", redirected.GetHeader("Location"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRedirectionFromScopedMount(TestEngine engine)
    {
        await using var upstream = new TestHost(Layout.Create().Build(), false, engine: engine);

        await upstream.Host.Handler(new ProxiedRouter(r =>
                          r.Respond().Header("Location", $"http://localhost:{upstream.Port}/target").Status(ResponseStatus.TemporaryRedirect).Build()))
                      .StartAsync();

        var proxy = Proxy.Create().Upstream("http://localhost:" + upstream.Port);

        await using var runner = await TestHost.RunAsync(Layout.Create().Add("api", proxy), engine: engine);

        using var redirected = await runner.GetResponseAsync("/api/whatever");

        var location = redirected.GetHeader("Location");

        Assert.IsNotNull(location);
        AssertX.Contains($"http://localhost:{runner.Port}", location);
        AssertX.Contains("/target", location);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestForwardingByAddressIsRelayed(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
        {
            var header = r.Header.Headers.GetEntry("Forwarded");

            Assert.IsNotNull(header);
            AssertX.Contains("by=203.0.113.5", header);

            return r.Respond().Content("Hello World!").Build();
        });

        var request = setup.Runner.GetRequest();
        request.Headers.Add("Forwarded", "for=85.192.1.5; by=203.0.113.5; host=google.com");

        using var response = await setup.Runner.GetResponseAsync(request);
        Assert.AreEqual("Hello World!", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestForwardingByAddressIsRelayedForIPv6(TestEngine engine)
    {
        await using var setup = await TestSetup.CreateAsync(engine, r =>
        {
            var header = r.Header.Headers.GetEntry("Forwarded");

            Assert.IsNotNull(header);
            AssertX.Contains("by=[2001:db8::1]", header);

            return r.Respond().Content("Hello World!").Build();
        });

        var request = setup.Runner.GetRequest();
        request.Headers.Add("Forwarded", "for=85.192.1.5; by=\"[2001:db8::1]\"; host=google.com");

        using var response = await setup.Runner.GetResponseAsync(request);
        Assert.AreEqual("Hello World!", await response.GetContentAsync());
    }

    [TestMethod]
    public void TestChaining()
    {
        Chain.Works(Proxy.Create().Upstream("https://google.com"));
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

        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request) => new ProxiedProvider(_response).HandleAsync(request);

    }

    private class ProxiedProvider : IHandler
    {
        private readonly Func<IRequest, IResponse?> _response;

        public ProxiedProvider(Func<IRequest, IResponse?> response)
        {
            _response = response;
        }

        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            Assert.IsNotNull(request.Client.Address);

            var response = _response.Invoke(request);

            if (response is not null)
            {
                return new ValueTask<IResponse?>(response);
            }

            var error = request.Respond()
                               .Status(ResponseStatus.InternalServerError)
                               .Build();

            return new(error);
        }
    }

    #endregion

}
