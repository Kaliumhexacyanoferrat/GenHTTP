using System.Net;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.ReverseProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cookie = GenHTTP.Api.Protocol.Cookie;

namespace GenHTTP.Testing.Acceptance.Modules;

[TestClass]
public sealed class ReverseProxyTests
{

    [TestMethod]
    public async Task TestBasics()
    {
        await using var setup = await TestSetup.CreateAsync(r => r.Respond().Content("Hello World!").Build());

        var runner = setup.Runner;

        using var response = await runner.GetResponseAsync();
        Assert.AreEqual("Hello World!", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task TestRedirection()
    {
        await using var setup = await TestSetup.CreateAsync(r =>
        {
            return r.Respond().Header("Location", $"http://localhost:{r.EndPoint.Port}/target").Status(ResponseStatus.TemporaryRedirect).Build();
        });

        var runner = setup.Runner;

        using var redirected = await runner.GetResponseAsync("/");

        Assert.AreEqual($"http://localhost:{runner.Port}/target", redirected.GetHeader("Location"));
    }

    [TestMethod]
    public async Task TestHead()
    {
        await using var setup = await TestSetup.CreateAsync(r =>
        {
            return r.Respond().Content("Hello World!").Build();
        });

        var runner = setup.Runner;

        var headRequest = runner.GetRequest();
        headRequest.Method = HttpMethod.Head;

        using var headed = await runner.GetResponseAsync(headRequest);

        await headed.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task TestCookies()
    {
        await using var setup = await TestSetup.CreateAsync(r =>
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
    public async Task TestHeaders()
    {
        var now = DateTime.UtcNow;

        await using var setup = await TestSetup.CreateAsync(r =>
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
    public async Task TestPost()
    {
        await using var setup = await TestSetup.CreateAsync(r =>
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
    public async Task TestPathing()
    {
        await using var setup = await TestSetup.CreateAsync(r =>
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
    public async Task TestQuery()
    {
        await using var setup = await TestSetup.CreateAsync(r =>
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
        await using var setup = await TestSetup.CreateAsync(r =>
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
        await using var setup = await TestSetup.CreateAsync(r =>
        {
            return r.Respond().Content(r.Target.Path.ToString(true)).Build();
        });

        var runner = setup.Runner;

        using var r = await runner.GetResponseAsync("/%3F%23%26%2F %20");
        Assert.AreEqual("/%3F%23%26%2F%20%20", await r.GetContentAsync());
    }

    [TestMethod]
    public async Task TestPathPreservesSpecialChars()
    {
        await using var setup = await TestSetup.CreateAsync(r =>
        {
            return r.Respond().Content(r.Target.Path.ToString(true)).Build();
        });

        var runner = setup.Runner;

        using var r = await runner.GetResponseAsync("/$@:");
        Assert.AreEqual("/$@:", await r.GetContentAsync());
    }

    [TestMethod]
    public async Task TestContentLengthPreserved()
    {
        await using var setup = await TestSetup.CreateAsync(r =>
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
    public async Task TestBadGateway()
    {
        var proxy = Proxy.Create()
                         .Upstream("http://icertainlydonotexistasadomain");

        await using var runner = await TestHost.RunAsync(proxy);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.BadGateway);
    }


    [TestMethod]
    public async Task TestCompression()
    {
        await using var setup = await TestSetup.CreateAsync(r =>
        {
            return r.Respond().Content("Hello World!").Build();
        });

        var runner = setup.Runner;

        var request = runner.GetRequest();

        request.Headers.Add("Accept-Encoding", "br, gzip, deflate");

        using var response = await runner.GetResponseAsync(request);

        Assert.AreEqual("br", response.GetContentHeader("Content-Encoding"));
    }

    #region Supporting data structures

    private class TestSetup : IAsyncDisposable
    {
        private readonly TestHost _Target;

        private TestSetup(TestHost source, TestHost target)
        {
            Runner = source;
            _Target = target;
        }

        public TestHost Runner { get; }

        public static async Task<TestSetup> CreateAsync(Func<IRequest, IResponse?> response)
        {
            // server hosting the actual web app
            var testServer = new TestHost(Layout.Create().Build(), false);

            await testServer.Host.Handler(new ProxiedRouter(response))
                      .StartAsync();

            // proxying server
            var proxy = Proxy.Create()
                             .ConnectTimeout(TimeSpan.FromSeconds(2))
                             .ReadTimeout(TimeSpan.FromSeconds(5))
                             .Upstream("http://localhost:" + testServer.Port);

            var runner = new TestHost(Layout.Create().Build());

            await runner.Host.Handler(proxy)
                  .StartAsync();

            return new TestSetup(runner, testServer);
        }

        #region IDisposable Support

        private bool _DisposedValue;

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!_DisposedValue)
            {
                if (disposing)
                {
                    await Runner.DisposeAsync();
                    await _Target.DisposeAsync();
                }

                _DisposedValue = true;
            }
        }

        public ValueTask DisposeAsync() => DisposeAsync(true);

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

        public ValueTask<IResponse?> HandleAsync(IRequest request) => new ProxiedProvider(_Response).HandleAsync(request);

    }

    private class ProxiedProvider : IHandler
    {
        private readonly Func<IRequest, IResponse?> _Response;

        public ProxiedProvider(Func<IRequest, IResponse?> response)
        {
            _Response = response;
        }

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

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

}
