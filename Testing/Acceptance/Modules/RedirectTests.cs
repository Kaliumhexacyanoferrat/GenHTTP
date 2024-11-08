using System.Net;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules;

[TestClass]
public sealed class RedirectTests
{

    [TestMethod]
    public async Task TestTemporary()
    {
        var redirect = Redirect.To("https://google.de/", true);

        await using var runner = await TestHost.RunAsync(redirect);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.TemporaryRedirect);
        Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
    }

    [TestMethod]
    public async Task TestTemporaryPost()
    {
        var redirect = Redirect.To("https://google.de/", true);

        await using var runner = await TestHost.RunAsync(redirect);

        var request = runner.GetRequest();
        request.Method = HttpMethod.Post;

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.SeeOther);
        Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
    }

    [TestMethod]
    public async Task TestPermanent()
    {
        var redirect = Redirect.To("https://google.de/");

        await using var runner = await TestHost.RunAsync(redirect);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.MovedPermanently);
        Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
    }

    [TestMethod]
    public async Task TestPermanentPost()
    {
        var redirect = Redirect.To("https://google.de/");

        await using var runner = await TestHost.RunAsync(redirect);

        var request = runner.GetRequest();
        request.Method = HttpMethod.Post;

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.PermanentRedirect);
        Assert.AreEqual("https://google.de/", response.GetHeader("Location"));
    }

    [TestMethod]
    public async Task TestAbsoluteRoute()
    {
        var layout = Layout.Create()
                           .Add("redirect", Redirect.To("/me/to/"));

        await using var runner = await TestHost.RunAsync(layout);

        using var response = await runner.GetResponseAsync("/redirect/");

        Assert.AreEqual("/me/to/", new Uri(response.GetHeader("Location")!).AbsolutePath);
    }
}
