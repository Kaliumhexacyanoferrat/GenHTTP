using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Controllers.Provider;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.DependencyInjection.Infrastructure;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.DependencyInjection;

public static class DependentController
{

    /// <summary>
    /// Adds the given controller to the layout with dependency injection enabled.
    /// </summary>
    /// <param name="layout">The layout to add the controller to</param>
    /// <param name="path">The path to register the controller at</param>
    /// <param name="injectors">Additional parameter injections to be used by the controller</param>
    /// <param name="serializers">Additional serializers to be used by the controller</param>
    /// <param name="formatters">Additional formatters to be used by the controller</param>
    /// <typeparam name="T">The class implementing the controller</typeparam>
    /// <returns>The given layout with the specified controller attached</returns>
    public static LayoutBuilder AddDependentController<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this LayoutBuilder layout, string path, InjectionRegistryBuilder? injectors = null, IBuilder<SerializationRegistry>? serializers = null, IBuilder<FormatterRegistry>? formatters = null) where T : class
    {
        var builder = new ControllerBuilder();

        builder.Type(typeof(T));
        builder.InstanceProvider(async (r) => await InstanceProvider.ProvideAsync<T>(r));

        builder.Configure(injectors, serializers, formatters);

        return layout.Add(path, builder);
    }

}
