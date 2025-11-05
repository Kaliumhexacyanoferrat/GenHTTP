using System.Net;

using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Redirects;

namespace GenHTTP.Testing.Acceptance.Modules.Redirects;

[TestClass]
public sealed class RedirectTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestTemporary(TestEngine engine)
    {
        var redirect = Redirect.To("https://google.de/", true);

        await using var runner = await TestHost.RunAsync(redirect, engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.TemporaryRedirect);
        Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestTemporaryPost(TestEngine engine)
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
    public async Task TestPermanent(TestEngine engine)
    {
        var redirect = Redirect.To("https://google.de/");

        await using var runner = await TestHost.RunAsync(redirect, engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.MovedPermanently);
        Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPermanentPost(TestEngine engine)
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
    public async Task TestAbsoluteRoute(TestEngine engine)
    {
        var layout = Layout.Create()
                           .Add("redirect", Redirect.To("/me/to/"));

        await using var runner = await TestHost.RunAsync(layout, engine: engine);

        using var response = await runner.GetResponseAsync("/redirect/");

        Assert.AreEqual("/me/to/", new Uri(response.GetHeader("Location")!).AbsolutePath);
    }

}
