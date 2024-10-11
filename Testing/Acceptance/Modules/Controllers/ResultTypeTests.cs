using System.Net;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers;

[TestClass]
public sealed class ResultTypeTests
{

    #region Helpers

    private static TestHost GetRunner()
    {
        var controller = Controller.From<TestController>()
                                   .Serializers(Serialization.Default())
                                   .Injectors(Injection.Default());

        return TestHost.Run(Layout.Create().Add("t", controller));
    }

    #endregion

    #region Supporting data structures

    public sealed class TestController
    {

        public IHandlerBuilder HandlerBuilder() => Content.From(Resource.FromString("HandlerBuilder"));

        public IHandler Handler(IHandler parent) => Content.From(Resource.FromString("Handler")).Build(parent);

        public IResponseBuilder ResponseBuilder(IRequest request) => request.Respond().Content("ResponseBuilder");

        public IResponse Response(IRequest request) => request.Respond().Content("Response").Build();
    }

    #endregion

    #region Tests

    [TestMethod]
    public async Task ControllerMayReturnHandlerBuilder()
    {
        using var runner = GetRunner();

        using var response = await runner.GetResponseAsync("/t/handler-builder/");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("HandlerBuilder", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task ControllerMayReturnHandler()
    {
        using var runner = GetRunner();

        using var response = await runner.GetResponseAsync("/t/handler/");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("Handler", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task ControllerMayReturnResponseBuilder()
    {
        using var runner = GetRunner();

        using var response = await runner.GetResponseAsync("/t/response-builder/");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("ResponseBuilder", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task ControllerMayReturnResponse()
    {
        using var runner = GetRunner();

        using var response = await runner.GetResponseAsync("/t/response/");

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual("Response", await response.GetContentAsync());
    }

    #endregion

}
