using GenHTTP.Modules.DependencyInjection.Framework;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Functional.Provider;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Modules.DependencyInjection;

public class DependentInline
{

    public static InlineBuilder Create() => Inline.Create().Injectors(Injection.Default().Add(new DependencyInjector()));

}
