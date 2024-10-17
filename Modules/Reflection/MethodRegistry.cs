using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Reflection;

/// <summary>
/// Composite structure to reference all customizable registries used by the
/// reflection functionality.
/// </summary>
/// <param name="Serialization">The serialization registry to be used</param>
/// <param name="Injection">The injection registry to be used</param>
/// <param name="Formatting">The formatter registry to be used</param>
public record MethodRegistry
(
    SerializationRegistry Serialization,
    InjectionRegistry Injection,
    FormatterRegistry Formatting
);
