using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.Webservices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

[TestClass]
public class IntegrationTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestWebserviceSupported(TestEngine engine)
    {
        var api = Layout.Create()
                        .AddService<MyService>("my")
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync(engine)).Document;

        Assert.AreEqual("/my/", doc?.Paths.First().Key);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestControllerSupported(TestEngine engine)
    {
        var api = Layout.Create()
                        .AddController<MyController>("my")
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync(engine)).Document;

        Assert.AreEqual("/my/method/", doc?.Paths.First().Key);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestInlineSupported(TestEngine engine)
    {
        var api = Inline.Create()
                        .Get("/method", () => 42)
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync(engine)).Document;

        Assert.AreEqual("/method", doc?.Paths.First().Key);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestObsoleteOperation(TestEngine engine)
    {
        var api = Layout.Create()
                        .AddService<ObsoleteService>("my")
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync(engine)).Document;

        Assert.IsTrue(doc?.Paths.First().Value.Operations?.First().Value.Deprecated);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestControllerWithMultipleMethods(TestEngine engine)
    {
        var api = Layout.Create()
                        .AddController<MultipleMethodsController>("my")
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync(engine)).Document;

        Assert.AreEqual(2, doc?.Paths?["/my/method/"].Operations?.Count);
    }

    #region Supporting data structures

    public class MyService
    {

        [ResourceMethod]
        public int Method() => 42;
    }

    public class ObsoleteService
    {

        [Obsolete("Don't use this anymore")]
        [ResourceMethod("obsolete")]
        public int Obsolete() => 43;
    }

    public class MyController
    {

        [ControllerAction(RequestMethod.Get)]
        public int Method() => 42;
    }

    public class MultipleMethodsController
    {

        [ControllerAction(RequestMethod.Get, RequestMethod.Options)]
        public int Method() => 42;
    }

    #endregion

}
