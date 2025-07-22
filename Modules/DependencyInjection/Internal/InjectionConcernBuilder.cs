using GenHTTP.Api.Content;

namespace GenHTTP.Modules.DependencyInjection.Internal;

public class InjectionConcernBuilder : IConcernBuilder
{
    private readonly IServiceProvider _Services;

    public InjectionConcernBuilder(IServiceProvider services)
    {
        _Services = services;
    }

    public IConcern Build(IHandler content) => new InjectionConcern(content, _Services);

}
