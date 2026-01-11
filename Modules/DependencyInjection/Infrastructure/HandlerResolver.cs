using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.DependencyInjection.Infrastructure;

internal static class HandlerResolver
{

    internal static T Obtain<T>(IRequest request)
    {
        var scope = request.GetServiceScope();

        var resolved = scope.ServiceProvider.GetService(typeof(T));

        if (resolved is T typed)
        {
            return typed;
        }

        throw new InvalidOperationException($"Unable to retrieve handler of type '{typeof(T)}' from service scope.");
    }

}
