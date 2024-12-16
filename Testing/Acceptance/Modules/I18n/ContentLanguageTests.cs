using System.Globalization;
using System.Net;
using GenHTTP.Modules.I18n;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.I18n;

[TestClass]
public class ContentLanguageTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestContentLanguage(TestEngine engine)
    {
        var app = Layout.Create()
                        .Add(Content.From(Resource.FromString("Hello World")))
                        .Add(Localization.Create().FromQuery());

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync("/?lang=de");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("de-DE", response.GetContentHeader("Content-Language"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestInvariantNoLanguage(TestEngine engine)
    {
        var app = Layout.Create()
                        .Add(Content.From(Resource.FromString("Hello World")))
                        .Add(Localization.Create().Default(CultureInfo.InvariantCulture));

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.IsNull(response.GetContentHeader("Content-Language"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNotFoundNoLanguage(TestEngine engine)
    {
        var app = Layout.Create()
                        .Add(Localization.Create().FromQuery());

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var response = await host.GetResponseAsync("/?lang=de");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);

        Assert.IsNull(response.GetContentHeader("Content-Language"));
    }

}
