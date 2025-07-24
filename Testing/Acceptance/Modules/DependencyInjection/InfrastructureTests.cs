using System.Net;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.Functional;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.DependencyInjection;

[TestClass]
public class InfrastructureTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServiceAvailability(TestEngine engine)
    {
        var app = Inline.Create()
                        .Get((IRequest r) =>
                        {
                            Assert.IsNotNull(r.GetServiceProvider());
                            Assert.IsNotNull(r.GetServiceScope());
                        });

        await using var runner = await DependentHost.RunAsync(app, engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task TestMeaningfulErrorIfNotConfigured()
    {
        var app = Inline.Create()
                        .Get((IRequest r) =>
                        {
                            Assert.Throws<InvalidOperationException>(r.GetServiceProvider);
                            Assert.Throws<InvalidOperationException>(r.GetServiceScope);
                        });

        await using var runner = await TestHost.RunAsync(app);

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

}
