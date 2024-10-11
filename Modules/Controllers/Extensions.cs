using System.Diagnostics.CodeAnalysis;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Controllers.Provider;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Controllers;

public static class Extensions
{

    /// <summary>
    /// Causes all requests to the specified path to be handled by the
    /// given controller class.
    /// </summary>
    /// <typeparam name="T">The type of the controller used to handle requests</typeparam>
    /// <param name="builder">The layout the controller should be added to</param>
    /// <param name="path">The path that should be handled by the controller</param>
    /// <param name="injectors">Optionally the injectors to be used by this controller</param>
    /// <param name="serializers">Optionally the serializers to be used by this controller</param>
    /// <param name="formatters">Optionally the formatters to be used by this controller</param>
    public static LayoutBuilder AddController<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this LayoutBuilder builder, string path, IBuilder<InjectionRegistry>? injectors = null, IBuilder<SerializationRegistry>? serializers = null, IBuilder<FormatterRegistry>? formatters = null) where T : new()
    {
        builder.Add(path, Controller.From<T>().Configured(injectors, serializers, formatters));
        return builder;
    }

    /// <summary>
    /// Causes the specified controller class to be used to handle the index of
    /// this layout.
    /// </summary>
    /// <typeparam name="T">The type of the controller used to handle requests</typeparam>
    /// <param name="builder">The layout the controller should be added to</param>
    /// <param name="injectors">Optionally the injectors to be used by this controller</param>
    /// <param name="serializers">Optionally the serializers to be used by this controller</param>
    /// <param name="formatters">Optionally the formatters to be used by this controller</param>
    public static LayoutBuilder IndexController<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this LayoutBuilder builder, IBuilder<InjectionRegistry>? injectors = null, IBuilder<SerializationRegistry>? serializers = null, IBuilder<FormatterRegistry>? formatters = null) where T : new()
    {
        builder.Add(Controller.From<T>().Configured(injectors, serializers, formatters));
        return builder;
    }

    private static ControllerBuilder Configured(this ControllerBuilder builder, IBuilder<InjectionRegistry>? injectors = null, IBuilder<SerializationRegistry>? serializers = null, IBuilder<FormatterRegistry>? formatters = null)
    {
        if (injectors != null)
        {
            builder.Injectors(injectors);
        }

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
