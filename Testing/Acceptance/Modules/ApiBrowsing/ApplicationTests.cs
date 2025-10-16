using System.Net;
using GenHTTP.Modules.ApiBrowsing;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.OpenApi;

namespace GenHTTP.Testing.Acceptance.Modules.ApiBrowsing;

[TestClass]
public class ApplicationTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSwagger(TestEngine engine)
    {
        var app = Layout.Create()
                        .Add(Inline.Create().Get(() => 42))
                        .AddOpenApi()
                        .AddSwaggerUI();

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync("/swagger/");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        AssertX.Contains("Swagger", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRedoc(TestEngine engine)
    {
        var app = Layout.Create()
                        .Add(Inline.Create().Get(() => 42))
                        .AddOpenApi()
                        .AddRedoc();

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync("/redoc/");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        AssertX.Contains("Redoc", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestScalar(TestEngine engine)
    {
        var app = Layout.Create()
                        .Add(Inline.Create().Get(() => 42))
                        .AddOpenApi()
                        .AddScalar();

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync("/scalar/");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        AssertX.Contains("Scalar", await response.GetContentAsync());
    }

}
