using System.Net;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Security.Cors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Security;

[TestClass]
public sealed class CorsTests
{

    [TestMethod]
    public async Task TestPreflight()
    {
        await using var runner = await GetRunnerAsync(CorsPolicy.Permissive());

        var request = new HttpRequestMessage(HttpMethod.Options, runner.GetUrl("/t"));

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task TestPermissive()
    {
        await using var runner = await GetRunnerAsync(CorsPolicy.Permissive());

        using var response = await runner.GetResponseAsync("/t");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("*", response.GetHeader("Access-Control-Allow-Origin"));

        Assert.AreEqual("*", response.GetHeader("Access-Control-Allow-Methods"));
        Assert.AreEqual("*, Authorization", response.GetHeader("Access-Control-Allow-Headers"));
        Assert.AreEqual("*", response.GetHeader("Access-Control-Expose-Headers"));

        Assert.AreEqual("true", response.GetHeader("Access-Control-Allow-Credentials"));

        Assert.AreEqual("86400", response.GetHeader("Access-Control-Max-Age"));

        Assert.AreEqual("Hello World", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task TestPermissiveWithoutDefaultAuthorizationHeader()
    {
        await using var runner = await GetRunnerAsync(CorsPolicy.Permissive(false));

        using var response = await runner.GetResponseAsync("/t");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("*", response.GetHeader("Access-Control-Allow-Origin"));

        Assert.AreEqual("*", response.GetHeader("Access-Control-Allow-Methods"));
        Assert.AreEqual("*", response.GetHeader("Access-Control-Allow-Headers"));
        Assert.AreEqual("*", response.GetHeader("Access-Control-Expose-Headers"));

        Assert.AreEqual("true", response.GetHeader("Access-Control-Allow-Credentials"));

        Assert.AreEqual("86400", response.GetHeader("Access-Control-Max-Age"));

        Assert.AreEqual("Hello World", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task TestRestrictive()
    {
        await using var runner = await GetRunnerAsync(CorsPolicy.Restrictive());

        using var response = await runner.GetResponseAsync("/t");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Origin"));

        Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Methods"));
        Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Headers"));
        Assert.IsFalse(response.Headers.Contains("Access-Control-Expose-Headers"));

        Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Credentials"));

        Assert.IsFalse(response.Headers.Contains("Access-Control-Max-Age"));
    }

    [TestMethod]
    public async Task TestCustom()
    {
        var policy = CorsPolicy.Restrictive()
                               .Add("http://google.de", new List<FlexibleRequestMethod>
                               {
                                   new(RequestMethod.Get)
                               }, null, new List<string>
                               {
                                   "Accept"
                               }, false);

        await using var runner = await GetRunnerAsync(policy);

        var request = runner.GetRequest("/t");
        request.Headers.Add("Origin", "http://google.de");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("http://google.de", response.GetHeader("Access-Control-Allow-Origin"));

        Assert.AreEqual("GET", response.GetHeader("Access-Control-Allow-Methods"));

        Assert.AreEqual("Accept", response.GetHeader("Access-Control-Expose-Headers"));

        Assert.AreEqual("Origin", response.GetHeader("Vary"));
    }

    private static async Task<TestHost> GetRunnerAsync(CorsPolicyBuilder policy)
    {
        var handler = Layout.Create()
                            .Add("t", Content.From(Resource.FromString("Hello World")))
                            .Add(policy);

        return await TestHost.RunAsync(handler);
    }
}
