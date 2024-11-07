using System.Net;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.Functional;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.ErrorHandling;

[TestClass]
public sealed class StructuredErrorMapperTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNotFound(TestEngine engine)
    {
        using var host = TestHost.Run(Inline.Create(), engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.NotFound);

        var model = await response.GetContentAsync<StructuredErrorMapper.ErrorModel>();

        Assert.AreEqual(ResponseStatus.NotFound, model.Status);

        Assert.IsNull(model.StackTrace);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGeneralError(TestEngine engine)
    {
        var handler = Inline.Create()
                            .Get(() => DoThrow(new Exception("Oops")));

        using var host = TestHost.Run(handler, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.InternalServerError);

        var model = await response.GetContentAsync<StructuredErrorMapper.ErrorModel>();

        Assert.AreEqual(ResponseStatus.InternalServerError, model.Status);

        Assert.IsNotNull(model.StackTrace);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestProviderError(TestEngine engine)
    {
        var handler = Inline.Create()
                            .Get(() => DoThrow(new ProviderException(ResponseStatus.Locked, "Locked up!")));

        using var host = TestHost.Run(handler, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.Locked);

        var model = await response.GetContentAsync<StructuredErrorMapper.ErrorModel>();

        Assert.AreEqual(ResponseStatus.Locked, model.Status);

        Assert.IsNotNull(model.StackTrace);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoTraceInProduction(TestEngine engine)
    {
        var handler = Inline.Create()
                            .Get(() => DoThrow(new Exception("Oops")));

        using var host = TestHost.Run(handler, development: false, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.InternalServerError);

        var model = await response.GetContentAsync<StructuredErrorMapper.ErrorModel>();

        Assert.IsNull(model.StackTrace);
    }

    private static void DoThrow(Exception e)
    {
        throw e;
    }

}
