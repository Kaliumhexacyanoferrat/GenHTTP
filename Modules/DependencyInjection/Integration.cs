using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.DependencyInjection.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Modules.DependencyInjection;

public static class Integration
{
    private const string ProviderVar = "__DI_SERVICE_PROVIDER";

    private const string ScopeVar = "__DI_SERVICE_SCOPE";

    /// <summary>
    /// Uses the given provider to injected dependencies within the server instance.
    /// </summary>
    /// <param name="host">The host to add dependency injection to</param>
    /// <param name="services">The service collection to be used for injection</param>
    /// <returns>The updated host</returns>
    public static IServerHost AddDependencyInjection(this IServerHost host, IServiceProvider services) => host.Add(new InjectionConcernBuilder(services));

    internal static void Configure(this IRequest request, IServiceProvider provider, IServiceScope scope)
    {
        request.Properties[ProviderVar] = provider;
        request.Properties[ScopeVar] = scope;
    }

    /// <summary>
    /// Retrieves the service provider used to resolve dependencies from the given request.
    /// </summary>
    /// <param name="request">The request to obtain the service provider from</param>
    /// <returns>The service provider retrieved from the request</returns>
    /// <exception cref="InvalidOperationException">Thrown if dependency injection is not enabled on the host</exception>
    public static IServiceProvider GetServiceProvider(this IRequest request)
    {
        if (!request.Properties.TryGet(ScopeVar, out IServiceProvider? provider))
        {
            throw new InvalidOperationException("Unable to retrieve service provider from the request. Ensure dependency injection has been configured.");
        }

        return provider!;
    }

    /// <summary>
    /// Retrieves the service scope used to resolve dependencies from the given request.
    /// </summary>
    /// <param name="request">The request to obtain the service scope from</param>
    /// <returns>The service scope retrieved from the request</returns>
    /// <exception cref="InvalidOperationException">Thrown if dependency injection is not enabled on the host</exception>
    public static IServiceScope GetServiceScope(this IRequest request)
    {
        if (!request.Properties.TryGet(ScopeVar, out IServiceScope? scope))
        {
            throw new InvalidOperationException("Unable to retrieve service scope from the request. Ensure dependency injection has been configured.");
        }

        return scope!;
    }

}
