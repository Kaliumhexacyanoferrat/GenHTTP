using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.DependencyInjection.Infrastructure;

namespace GenHTTP.Modules.DependencyInjection.Basics;

internal class HandlerIntegration<T> : IHandler where T: class, IDependentHandler
{

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var instance = await InstanceProvider.ProvideAsync<T>(request);

        return await instance.HandleAsync(request);
    }

}
