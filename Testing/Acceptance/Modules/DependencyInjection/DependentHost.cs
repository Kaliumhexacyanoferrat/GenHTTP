using GenHTTP.Api.Content;

using GenHTTP.Modules.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Testing.Acceptance.Modules.DependencyInjection;

public static class DependentHost
{

    public static async Task<TestHost> RunAsync(IHandlerBuilder app, IServiceProvider? services = null, TestEngine engine = TestEngine.Internal)
    {
        var host = new TestHost(app.Build(), engine: engine);

        host.Host.AddDependencyInjection(services ?? new ServiceCollection().BuildServiceProvider());

        await host.StartAsync();

        return host;
    }

}
