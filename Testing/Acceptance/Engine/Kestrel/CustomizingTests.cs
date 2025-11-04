using GenHTTP.Engine.Kestrel;
using GenHTTP.Modules.Layouting;
using Microsoft.AspNetCore.Builder;

namespace GenHTTP.Testing.Acceptance.Engine.Kestrel;

[TestClass]
public class CustomizingTests
{

    [TestMethod]
    public async Task TestHooks()
    {
        if (!Engines.KestrelEnabled()) return;

        var configHook = (WebApplicationBuilder b) => { };

        var appHook = (WebApplication a) => { };

        var host = Host.Create(configHook, appHook).Handler(Layout.Create());

        await host.StartAsync();

        await host.StopAsync();
    }

}
