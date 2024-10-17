using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Reflection;

public record MethodRegistry
(
    SerializationRegistry Serialization,
    InjectionRegistry Injection,
    FormatterRegistry Formatting
);
