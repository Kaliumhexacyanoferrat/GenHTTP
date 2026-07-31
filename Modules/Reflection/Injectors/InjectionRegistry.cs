namespace GenHTTP.Modules.Reflection.Injectors;

public sealed class InjectionRegistry : List<IParameterInjector>
{

    #region Initialization

    public InjectionRegistry(IEnumerable<IParameterInjector> injectors) : base(injectors)
    {

    }

    #endregion

}
