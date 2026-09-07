using System.Net;
using System.Net.Http.Headers;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Testing.Acceptance.Modules.Functional;

[TestClass]
public class MethodTest
{

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestAnyMethod(ServerEngine engine, ExecutionMode mode)
    {
        var app = Inline.Create()
                        .Any((List<int> data) => data.Count)
                        .ExecutionMode(mode);

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
    [MultiEngineFrameworkTest]
    public async Task TestDelete(ServerEngine engine, ExecutionMode mode)
    {
        var app = Inline.Create()
                        .Delete(() => { })
                        .ExecutionMode(mode);

        await using var host = await TestHost.RunAsync(app, engine: engine);

        var request = host.GetRequest(method: HttpMethod.Delete);

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.NoContent);
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestHead(ServerEngine engine, ExecutionMode mode)
    {
        var app = Inline.Create()
                        .Head(() => "42")
                        .ExecutionMode(mode);

        await using var host = await TestHost.RunAsync(app, engine: engine);

        var request = host.GetRequest(method: HttpMethod.Head);

        using var response = await host.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("2", response.GetContentHeader("Content-Length"));
    }

}
