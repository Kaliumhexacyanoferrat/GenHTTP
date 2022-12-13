using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Reflection
{

    public static class Injection
    {

        public static InjectionRegistryBuilder Empty() => new InjectionRegistryBuilder();

        public static InjectionRegistryBuilder Default()
        {
            return new InjectionRegistryBuilder().Add(new RequestInjector())
                                                 .Add(new RequestBodyInjector())
                                                 .Add(new HandlerInjector());
        }

    }

}
