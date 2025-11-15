using GenHTTP.Api.Content;

namespace GenHTTP.Modules.DependencyInjection.Infrastructure;

internal class InjectionConcernBuilder : IConcernBuilder
{
    private readonly IServiceProvider _services;

    #region Initialization

    internal InjectionConcernBuilder(IServiceProvider services)
    {
        _services = services;
    }

    #endregion

    public IConcern Build(IHandler content) => new InjectionConcern(content, _services);

}
