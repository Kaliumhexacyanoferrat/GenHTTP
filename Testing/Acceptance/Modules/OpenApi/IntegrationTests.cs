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

    #endregion

    [TestMethod]
    public async Task TestWebserviceSupported()
    {
        var api = Layout.Create()
                        .AddService<MyService>("my")
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync()).OpenApiDocument;

        Assert.AreEqual("/my/", doc.Paths.First().Key);
    }

    [TestMethod]
    public async Task TestControllerSupported()
    {
        var api = Layout.Create()
                        .AddController<MyController>("my")
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync()).OpenApiDocument;

        Assert.AreEqual("/my/method/", doc.Paths.First().Key);
    }

    [TestMethod]
    public async Task TestInlineSupported()
    {
        var api = Inline.Create()
                        .Get("/method", () => 42)
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync()).OpenApiDocument;

        Assert.AreEqual("/method", doc.Paths.First().Key);
    }

    [TestMethod]
    public async Task TestObsoleteOperation()
    {
        var api = Layout.Create()
                        .AddService<ObsoleteService>("my")
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync()).OpenApiDocument;

        Assert.IsTrue(doc.Paths.First().Value.Operations.First().Value.Deprecated);
    }

}
