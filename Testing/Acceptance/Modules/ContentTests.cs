using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules;

[TestClass]
public sealed class ContentTests
{

    [TestMethod]
    public async Task TestDeserialization()
    {
        var expectation = new MyType(42);

        var handler = Inline.Create()
                            .Get(() => expectation);

        using var host = TestHost.Run(handler);

        using var response = await host.GetResponseAsync();

        Assert.AreEqual(expectation, await response.GetContentAsync<MyType>());
    }

    [TestMethod]
    public async Task TestNull()
    {
        var handler = Inline.Create()
                            .Get(() => (MyType?)null);

        using var host = TestHost.Run(handler);

        using var response = await host.GetResponseAsync();

        Assert.IsNull(await response.GetOptionalContentAsync<MyType>());
    }

    [TestMethod]
    public async Task TestUnsupported()
    {
        var handler = Inline.Create()
                            .Get(() => new Result<string>("Nah").Type(FlexibleContentType.Get("text/html")));

        using var host = TestHost.Run(handler);

        using var response = await host.GetResponseAsync();

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await response.GetOptionalContentAsync<MyType>();
        });
    }

    public record MyType(int Id);
}
