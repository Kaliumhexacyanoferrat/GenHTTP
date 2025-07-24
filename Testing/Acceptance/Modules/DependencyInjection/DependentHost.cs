using GenHTTP.Api.Content;

using GenHTTP.Modules.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Testing.Acceptance.Modules.DependencyInjection;

public static class DependentHost
{

    public static async Task<TestHost> RunAsync(IHandlerBuilder app, TestEngine engine = TestEngine.Internal)
    {
        var host = new TestHost(app.Build(), engine: engine);

        var services = new ServiceCollection();

        services.AddSingleton<AwesomeService>();

        host.Host.AddDependencyInjection(services.BuildServiceProvider());

        await host.StartAsync();

        return host;
    }

}
