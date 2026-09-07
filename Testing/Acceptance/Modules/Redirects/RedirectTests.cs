using System.Net;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Redirects;

namespace GenHTTP.Testing.Acceptance.Modules.Redirects;

[TestClass]
public sealed class RedirectTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestTemporary(ServerEngine engine)
    {
        var redirect = Redirect.To("https://google.de/", true);

        await using var runner = await TestHost.RunAsync(redirect, engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.TemporaryRedirect);
        Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestTemporaryPost(ServerEngine engine)
    {
        var redirect = Redirect.To("https://google.de/", true);

        await using var runner = await TestHost.RunAsync(redirect, engine: engine);

        var request = runner.GetRequest();
        request.Method = HttpMethod.Post;

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.SeeOther);
        Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPermanent(ServerEngine engine)
    {
        var redirect = Redirect.To("https://google.de/");

        await using var runner = await TestHost.RunAsync(redirect, engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.MovedPermanently);
        Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPermanentPost(ServerEngine engine)
    {
        var redirect = Redirect.To("https://google.de/");

        await using var runner = await TestHost.RunAsync(redirect, engine: engine);

        var request = runner.GetRequest();
        request.Method = HttpMethod.Post;

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.PermanentRedirect);
        Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAbsoluteRoute(ServerEngine engine)
    {
        var layout = Layout.Create()
                           .Add("redirect", Redirect.To("/me/to/"));

        await using var runner = await TestHost.RunAsync(layout, engine: engine);

        using var response = await runner.GetResponseAsync("/redirect/");

        Assert.AreEqual("/me/to/", new Uri(response.GetHeader("Location")!).AbsolutePath);
    }

}
