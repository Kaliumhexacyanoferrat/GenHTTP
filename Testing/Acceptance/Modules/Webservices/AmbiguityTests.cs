using System.Net;
using GenHTTP.Api.Content;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Webservices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Webservices;

[TestClass]
public sealed class AmbiguityTests
{

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSpecificPreferred(TestEngine engine)
    {
        var app = Layout.Create()
                        .AddService<TestService>("c");

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync("/c/my.txt");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Specific", await response.GetContentAsync());
    }

    #endregion

    #region Supporting data structures

    public sealed class TestService
    {

        [ResourceMethod]
        public IHandlerBuilder Wildcard() => Content.From(Resource.FromString("Wildcard"));

        [ResourceMethod(path: "/my.txt")]
        public string Specific() => "Specific";
    }

    #endregion

}
