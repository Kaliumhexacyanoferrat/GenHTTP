using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Reflection.Injectors;

public class InjectionRegistryBuilder : IBuilder<InjectionRegistry>
{
    private readonly List<IParameterInjector> _injectors = [];

    #region Functionality

    public InjectionRegistryBuilder Add(IParameterInjector injector)
    {
        _injectors.Add(injector);
        return this;
    }

    public InjectionRegistry Build() => new(_injectors);

    #endregion

}
