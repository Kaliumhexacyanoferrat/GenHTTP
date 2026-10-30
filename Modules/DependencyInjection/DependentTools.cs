using GenHTTP.Modules.DependencyInjection.Infrastructure;
using GenHTTP.Modules.Mcp;
using GenHTTP.Modules.Mcp.Provider;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.DependencyInjection;

public static class DependentTools
{

    /// <summary>
    /// Creates an endpoint providing tools to AI agents, with tool classes being
    /// created per call with their dependencies resolved from the service scope of
    /// the request, and services being injected into the parameters of tools.
    /// </summary>
    /// <param name="injectors">The injectors to be used in addition to dependency injection</param>
    public static ToolsHandlerBuilder Create(InjectionRegistryBuilder? injectors = null)
    {
        var registry = injectors ?? Injection.Default();

        registry.Add(new DependencyInjector());

        return Tools.Create()
                    .Injectors(registry)
                    .InstanceProvider(InstanceProvider.ProvideAsync);
    }

}
