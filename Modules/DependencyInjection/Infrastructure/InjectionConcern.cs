using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Modules.DependencyInjection.Infrastructure;

public class InjectionConcern : IConcern
{

    public IHandler Content { get; }

    public IServiceProvider Services { get; }

    public InjectionConcern(IHandler content, IServiceProvider services)
    {
        Content = content;
        Services = services;
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        using var scope = Services.CreateScope();

        request.Configure(Services, scope);

        return Content.HandleAsync(request);
    }

}
