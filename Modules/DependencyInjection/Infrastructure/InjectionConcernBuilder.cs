using GenHTTP.Api.Content;

namespace GenHTTP.Modules.DependencyInjection.Infrastructure;

internal class InjectionConcernBuilder : IConcernBuilder
{
    private readonly IServiceProvider _Services;

    #region Initialization

    internal InjectionConcernBuilder(IServiceProvider services)
    {
        _Services = services;
    }

    #endregion

    public IConcern Build(IHandler content) => new InjectionConcern(content, _Services);

}
