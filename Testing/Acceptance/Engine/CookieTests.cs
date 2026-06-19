using GenHTTP.Api.Protocol;

using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class CookieTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCookieCanBeRead(TestEngine engine)
    {
        string? cookie = null;

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            cookie = r.Header.Headers.GetCookie("session");

            return r.Respond().Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Cookie", "first=1; session=abc123; third=3");

        using var _ = await runner.GetResponseAsync(request);

        Assert.AreEqual("abc123", cookie);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestMissingCookieReturnsNull(TestEngine engine)
    {
        string? cookie = "not-null";

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            cookie = r.Header.Headers.GetCookie("missing");

            return r.Respond().Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Cookie", "first=1; second=2");

        using var _ = await runner.GetResponseAsync(request);

        Assert.IsNull(cookie);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCookiesCanBeIterated(TestEngine engine)
    {
        List<(string, string)>? cookies = null;

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            var list = r.Header.Headers.GetCookies();

            cookies = [];

            for (var i = 0; i < list.Count; i++)
            {
                var entry = list[i];

                cookies.Add((entry.Key.ToString(), entry.Value.ToString()));
            }

            return r.Respond().Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Cookie", "first=1; second=2");

        using var _ = await runner.GetResponseAsync(request);

        CollectionAssert.AreEqual(new[] { ("first", "1"), ("second", "2") }, cookies);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCookiesCanBeLookedUpByName(TestEngine engine)
    {
        string? cookie = null;

        var handler = new FunctionalHandler(responseProvider: r =>
        {
            cookie = r.Header.Headers.GetCookies().GetEntry("second");

            return r.Respond().Build();
        });

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        var request = runner.GetRequest();
        request.Headers.Add("Cookie", "first=1; second=2");

        using var _ = await runner.GetResponseAsync(request);

        Assert.AreEqual("2", cookie);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCookieCanBeWritten(TestEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r => r.Respond().Cookie("session", "abc123").Build());

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        using var response = await runner.GetResponseAsync();

        response.Headers.TryGetValues("Set-Cookie", out var values);

        CollectionAssert.AreEqual(new[] { "session=abc123" }, values?.ToList());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCookieWithOptionsCanBeWritten(TestEngine engine)
    {
        var options = new CookieOptions
        {
            MaxAge = TimeSpan.FromSeconds(86400),
            Domain = new ByteString("example.com"),
            Path = new ByteString("/api"),
            Secure = true,
            HttpOnly = true,
            SameSite = SameSite.Strict
        };

        var handler = new FunctionalHandler(responseProvider: r => r.Respond().Cookie("session", "abc123", options).Build());

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        using var response = await runner.GetResponseAsync();

        response.Headers.TryGetValues("Set-Cookie", out var values);

        CollectionAssert.AreEqual(new[] { "session=abc123; Max-Age=86400; Domain=example.com; Path=/api; Secure; HttpOnly; SameSite=Strict" }, values?.ToList());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestMultipleCookiesCanBeWritten(TestEngine engine)
    {
        var handler = new FunctionalHandler(responseProvider: r => r.Respond().Cookie("first", "1").Cookie("second", "2").Build());

        await using var runner = await TestHost.RunAsync(handler.Wrap(), engine: engine);

        using var response = await runner.GetResponseAsync();

        response.Headers.TryGetValues("Set-Cookie", out var values);

        CollectionAssert.AreEqual(new[] { "first=1", "second=2" }, values?.ToList());
    }

}
