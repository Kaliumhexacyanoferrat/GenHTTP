using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.DependencyInjection.Internal;

public static class InstanceProvider
{

    public static ValueTask<object> Provide<T>(IRequest request) where T : class
    {
        var scope = request.GetScope();

        var instance = scope.ServiceProvider.GetService(typeof(T)) ?? throw new InvalidOperationException($"Service provider did not provide an instance for type '{typeof(T)}'");

        return ValueTask.FromResult(instance);
    }

}
