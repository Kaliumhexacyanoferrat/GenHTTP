using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.DependencyInjection.Infrastructure;

namespace GenHTTP.Modules.DependencyInjection.Basics;

public class ConcernIntegration<T> : IConcern where T : class, IDependentConcern
{

    public IHandler Content { get; }

    public ConcernIntegration(IHandler content)
    {
        Content = content;
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var instance = await InstanceProvider.ProvideAsync<T>(request);

        return await instance.HandleAsync(Content, request);
    }

}
