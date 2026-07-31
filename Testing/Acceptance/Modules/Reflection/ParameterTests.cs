using System.Net;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Testing.Acceptance.Modules.Reflection;

[TestClass]
public sealed class ParameterTests
{

    #region Tests

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestCanReadSimpleTypesFromBody(TestEngine engine, ExecutionMode mode)
    {
        var inline = Inline.Create()
                           .Post(([FromBody] string body) => body)
                           .ExecutionMode(mode);

        await using var runner = await TestHost.RunAsync(inline, engine: engine);

        using var response = await PostAsync(runner, "1");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("1", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestConversionError(TestEngine engine, ExecutionMode mode)
    {
        var inline = Inline.Create()
                           .Post(([FromBody] int number) => number)
                           .ExecutionMode(mode);

        await using var runner = await TestHost.RunAsync(inline, engine: engine);

        using var response = await PostAsync(runner, "ABC");

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    private static Task<HttpResponseMessage> PostAsync(TestHost host, string body)
    {
        var request = host.GetRequest();

        request.Method = HttpMethod.Post;
        request.Content = new StringContent(body, null, "text/plain");

        return host.GetResponseAsync(request);
    }

    #endregion

}
