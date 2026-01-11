using GenHTTP.Api.Protocol;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Modules.DependencyInjection.Infrastructure;

internal static class HandlerResolver
{

    internal static T Obtain<T>(IRequest request)
    {
        var scope = request.GetServiceScope();

        var instance = scope.ServiceProvider.GetService(typeof(T))
            ?? ActivatorUtilities.CreateInstance<T>(scope.ServiceProvider);

        if (instance is T typed)
        {
            return typed;
        }

        throw new InvalidOperationException($"Unable to retrieve handler of type '{typeof(T)}' from service scope.");
    }

}
