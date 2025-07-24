using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.DependencyInjection.Infrastructure;

namespace GenHTTP.Modules.DependencyInjection.Basics;

internal class ConcernIntegration<T> : IConcern where T : class, IDependentConcern
{

    #region Getters/Setters

    public IHandler Content { get; }

    #endregion

    #region Initialization

    internal ConcernIntegration(IHandler content)
    {
        Content = content;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var instance = await InstanceProvider.ProvideAsync<T>(request);

        return await instance.HandleAsync(Content, request);
    }

    #endregion

}
