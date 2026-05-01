using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Modules.DependencyInjection.Infrastructure;

internal class InjectionConcern : IConcern
{

    #region Get-/Setters

    public IHandler Content { get; }

    internal IServiceProvider Services { get; }

    #endregion

    #region Initialization

    internal InjectionConcern(IHandler content, IServiceProvider services)
    {
        Content = content;
        Services = services;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        using var scope = Services.CreateScope();

        request.Configure(Services, scope);

        return await Content.HandleAsync(request);
    }

    #endregion

}
