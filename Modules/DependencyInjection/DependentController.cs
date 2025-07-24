using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Controllers.Provider;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.DependencyInjection.Framework;
using GenHTTP.Modules.DependencyInjection.Infrastructure;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.DependencyInjection;

public static class DependentController
{

    public static LayoutBuilder AddDependentController<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this LayoutBuilder layout, string path, InjectionRegistryBuilder? injectors = null, IBuilder<SerializationRegistry>? serializers = null, IBuilder<FormatterRegistry>? formatters = null) where T : class
    {
        var builder = new ControllerBuilder();

        builder.Type(typeof(T));
        builder.InstanceProvider(async (r) => await InstanceProvider.ProvideAsync<T>(r));

        var injectionOverlay = injectors ?? Injection.Default();

        injectionOverlay.Add(new DependencyInjector());

        builder.Injectors(injectionOverlay);

        if (serializers != null)
        {
            builder.Serializers(serializers);
        }

        if (formatters != null)
        {
            builder.Formatters(formatters);
        }

        return layout.Add(path, builder);
    }

}
