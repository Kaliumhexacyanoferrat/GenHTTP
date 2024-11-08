using GenHTTP.Engine.Kestrel;
using GenHTTP.Modules.Layouting;
using Microsoft.AspNetCore.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine.Kestrel;

[TestClass]
public class CustomizingTests
{

    [TestMethod]
    public async Task TestHooks()
    {
        var configHook = (WebApplicationBuilder b) => { };

        var appHook = (WebApplication a) => { };

        var host = Host.Create(configHook, appHook).Handler(Layout.Create());

        await host.StartAsync();

        await host.StopAsync();
    }

}
