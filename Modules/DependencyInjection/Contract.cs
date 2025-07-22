using GenHTTP.Api.Protocol;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Modules.DependencyInjection;

public static class Contract
{
    private const string ProviderVar = "__DI_SERVICE_PROVIDER";

    private const string ScopeVar = "__DI_SERVICE_SCOPE";

    internal static void Configure(this IRequest request, IServiceProvider provider, IServiceScope scope)
    {
        request.Properties[ProviderVar] = provider;
        request.Properties[ScopeVar] = scope;
    }

    public static IServiceProvider GetServiceProvider(this IRequest request)
    {
        if (!request.Properties.TryGet(ScopeVar, out IServiceProvider? provider))
        {
            throw new InvalidOperationException("Unable to retrieve service provider from the request. Ensure dependency injection has been configured.");
        }

        return provider!;
    }

    public static IServiceScope GetServiceScope(this IRequest request)
    {
        if (!request.Properties.TryGet(ScopeVar, out IServiceScope? scope))
        {
            throw new InvalidOperationException("Unable to retrieve service scope from the request. Ensure dependency injection has been configured.");
        }

        return scope!;
    }

}
