using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.DependencyInjection.Infrastructure;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Functional.Provider;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.DependencyInjection;

public static class DependentInline
{

    /// <summary>
    /// Creates a new line handler with dependency injection enabled on the declared methods.
    /// </summary>
    /// <param name="injectors">Additional parameter injections to be used by the inline handler</param>
    /// <param name="serializers">Additional serializers to be used by the inline handler</param>
    /// <param name="formatters">Additional formatters to be used by the inline handler</param>
    /// <returns>The newly created inline handler</returns>
    public static InlineBuilder Create(InjectionRegistryBuilder? injectors = null, IBuilder<SerializationRegistry>? serializers = null, IBuilder<FormatterRegistry>? formatters = null)
        => Inline.Create().Configure(injectors, serializers, formatters);

}
