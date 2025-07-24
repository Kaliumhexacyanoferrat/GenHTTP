using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.DependencyInjection.Infrastructure;

internal static class FrameworkConfiguration
{

    internal static T Configure<T>(this T builder, InjectionRegistryBuilder? injectors = null, IBuilder<SerializationRegistry>? serializers = null, IBuilder<FormatterRegistry>? formatters = null) where T : IRegistryBuilder<T>
    {
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

        return builder;
    }

}
