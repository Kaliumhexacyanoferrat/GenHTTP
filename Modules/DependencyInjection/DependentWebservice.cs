using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.DependencyInjection.Infrastructure;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Reflection.Injectors;
using GenHTTP.Modules.Webservices.Provider;

namespace GenHTTP.Modules.DependencyInjection;

public static class DependentWebservice
{

    /// <summary>
    /// Adds the given webservice to the layout with dependency injection enabled.
    /// </summary>
    /// <param name="layout">The layout to add the webservice to</param>
    /// <param name="path">The path to register the webservice at</param>
    /// <param name="injectors">Additional parameter injections to be used by the webservice</param>
    /// <param name="serializers">Additional serializers to be used by the webservice</param>
    /// <param name="formatters">Additional formatters to be used by the webservice</param>
    /// <typeparam name="T">The class implementing the webservice</typeparam>
    /// <returns>The given layout with the specified webservice attached</returns>
    public static LayoutBuilder AddDependentService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this LayoutBuilder layout, string path, InjectionRegistryBuilder? injectors = null, IBuilder<SerializationRegistry>? serializers = null, IBuilder<FormatterRegistry>? formatters = null) where T : class
    {
        var builder = new ServiceResourceBuilder();

        builder.Type(typeof(T));
        builder.InstanceProvider(async (r) => await InstanceProvider.ProvideAsync<T>(r));

        builder.Configure(injectors, serializers, formatters);

        return layout.Add(path, builder);
    }

}
