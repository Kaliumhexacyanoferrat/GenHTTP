using System.Net;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.Functional;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.ErrorHandling;

[TestClass]
public class HtmlErrorMapperTest
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNotFound(TestEngine engine)
    {
        await using var host = await TestHost.RunAsync(Inline.Create().Add(ErrorHandler.Html()), engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGeneralError(TestEngine engine)
    {
        var handler = Inline.Create()
                            .Get(() => DoThrow(new Exception("Oops")))
                            .Add(ErrorHandler.Html());

        await using var host = await TestHost.RunAsync(handler, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestProviderError(TestEngine engine)
    {
        var handler = Inline.Create()
                            .Get(() => DoThrow(new ProviderException(ResponseStatus.Locked, "Locked up!")))
                            .Add(ErrorHandler.Html());

        await using var host = await TestHost.RunAsync(handler, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.Locked);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoTraceInProduction(TestEngine engine)
    {
        var handler = Inline.Create()
                            .Get(() => DoThrow(new Exception("Oops")))
                            .Add(ErrorHandler.Html());

        await using var host = await TestHost.RunAsync(handler, development: false, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.InternalServerError);
    }

    private static void DoThrow(Exception e)
    {
        throw e;
    }

}
