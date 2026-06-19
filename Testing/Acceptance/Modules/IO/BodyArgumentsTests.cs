using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public sealed class BodyArgumentsTests
{

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestArgumentsAreInjected(TestEngine engine, ExecutionMode mode)
    {
        var handler = Inline.Create()
                            .Post((BodyArguments args) => $"{args.GetEntry("name")}-{args.GetEntry("age")}")
                            .ExecutionMode(mode);

        await using var host = await TestHost.RunAsync(handler, engine: engine);

        using var response = await PostAsync(host, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["name"] = "John Doe",
            ["age"] = "42"
        }));

        Assert.AreEqual("John Doe-42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestEncodedCharactersAreDecoded(TestEngine engine, ExecutionMode mode)
    {
        var handler = Inline.Create()
                            .Post((BodyArguments args) => args.GetEntry("value"))
                            .ExecutionMode(mode);

        await using var host = await TestHost.RunAsync(handler, engine: engine);

        var content = new StringContent("value=a%2Bb%3Dc", null, "application/x-www-form-urlencoded");

        using var response = await PostAsync(host, content);

        Assert.AreEqual("a+b=c", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestNoBody(TestEngine engine, ExecutionMode mode)
    {
        var handler = Inline.Create()
                            .Get((BodyArguments args) => args.Count)
                            .ExecutionMode(mode);

        await using var host = await TestHost.RunAsync(handler, engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual("0", await response.GetContentAsync());
    }

    private static Task<HttpResponseMessage> PostAsync(TestHost host, HttpContent content)
    {
        var request = host.GetRequest(method: HttpMethod.Post);

        request.Content = content;

        return host.GetResponseAsync(request);
    }

}
