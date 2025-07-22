using GenHTTP.Api.Protocol;
using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Modules.DependencyInjection;

public static class Contract
{
    private const string ScopeVar = "di-scope";

    public static void SetScope(this IRequest request, IServiceScope scope)
    {
        request.Properties[ScopeVar] = scope;
    }

    public static IServiceScope GetScope(this IRequest request)
    {
        if (!request.Properties.TryGet(ScopeVar, out IServiceScope? scope))
        {
            throw new InvalidOperationException("Unable to retrieve service scope from the request. Ensure dependency injection has been configured.");
        }

        return scope!;
    }

}
