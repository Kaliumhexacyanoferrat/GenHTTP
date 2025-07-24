using GenHTTP.Modules.DependencyInjection.Infrastructure;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Functional.Provider;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Modules.DependencyInjection;

public static class DependentInline
{

    public static InlineBuilder Create() => Inline.Create().Injectors(Injection.Default().Add(new DependencyInjector()));

}
