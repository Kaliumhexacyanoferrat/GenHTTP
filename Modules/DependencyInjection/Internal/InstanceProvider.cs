using GenHTTP.Api.Protocol;
using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Modules.DependencyInjection.Internal;

public static class InstanceProvider
{

    public static ValueTask<object> Provide<T>(IRequest request) where T : class
    {
        var scope = request.GetServiceScope();

        var instance = scope.ServiceProvider.GetService(typeof(T))
            ?? ActivatorUtilities.CreateInstance<T>(scope.ServiceProvider);

        if (instance == null)
        {
            throw new InvalidOperationException($"Unable to resolve or construct instance of type '{typeof(T)}'");
        }

        return ValueTask.FromResult(instance);
    }

}
