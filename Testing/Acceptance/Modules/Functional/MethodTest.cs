using System.Net;
using System.Net.Http.Headers;
using GenHTTP.Modules.Functional;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Functional;

[TestClass]
public class MethodTest
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAnyMethod(TestEngine engine)
    {
        var app = Inline.Create()
                        .Any((List<int> data) => data.Count);

        await using var host = await TestHost.RunAsync(app, engine: engine);

        foreach (var method in new[]
                 {
                     HttpMethod.Post, HttpMethod.Put
                 })
        {
            var request = host.GetRequest(method: method);

            request.Content = new StringContent("[ 1, 2 ]");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            using var response = await host.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("2", await response.GetContentAsync());
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDelete(TestEngine engine)
    {
        var app = Inline.Create()
                        .Delete(() => { });

        await using var host = await TestHost.RunAsync(app, engine: engine);

        var request = host.GetRequest(method: HttpMethod.Delete);

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.NoContent);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestHead(TestEngine engine)
    {
        var app = Inline.Create()
                        .Head(() => "42");

        await using var host = await TestHost.RunAsync(app, engine: engine);

        var request = host.GetRequest(method: HttpMethod.Head);

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("2", response.GetContentHeader("Content-Length"));
    }

}
