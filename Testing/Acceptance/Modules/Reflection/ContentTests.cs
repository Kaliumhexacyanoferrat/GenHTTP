using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Reflection;

[TestClass]
public sealed class ContentTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDeserialization(TestEngine engine)
    {
        var expectation = new MyType(42);

        var handler = Inline.Create()
                            .Get(() => expectation);

        await using var host = await TestHost.RunAsync(handler, engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual(expectation, await response.GetContentAsync<MyType>());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNull(TestEngine engine)
    {
        var handler = Inline.Create()
                            .Get(() => (MyType?)null);

        await using var host = await TestHost.RunAsync(handler, engine: engine);

        using var response = await host.GetResponseAsync();

        Assert.IsNull(await response.GetOptionalContentAsync<MyType>());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUnsupported(TestEngine engine)
    {
        var handler = Inline.Create()
                            .Get(() => new Result<string>("Nah").Type(FlexibleContentType.Get("text/html")));

        await using var host = await TestHost.RunAsync(handler, engine: engine);

        using var response = await host.GetResponseAsync();

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
        {
            await response.GetOptionalContentAsync<MyType>();
        });
    }

    public record MyType(int Id);
}
