using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Reflection;

/// <summary>
/// A protocol for builders that will internally create a <see cref="MethodRegistry"/>
/// instance.
/// </summary>
/// <typeparam name="T">The builder type to be returned</typeparam>
public interface IRegistryBuilder<out T>
{

    /// <summary>
    /// Configures the serialization formats (such as JSON or XML) that are
    /// supported by this service to read or generate bodies.
    /// </summary>
    /// <param name="registry">The serialization registry to be used</param>
    T Serializers(IBuilder<SerializationRegistry> registry);

    /// <summary>
    /// Configures the parameter injectors that are available within this
    /// service. This allows you to inject custom values into the methods
    /// of your service, such as the currently logged-in user or session.
    /// </summary>
    /// <param name="registry">The injection registry to be used</param>
    T Injectors(IBuilder<InjectionRegistry> registry);

    /// <summary>
    /// Configures the formatters that will be used by this service to
    /// format primitive types (such as numbers or dates). This allows
    /// you to use custom formats within paths, queries or bodies.
    /// </summary>
    /// <param name="registry">The formatter registry to be used</param>
    T Formatters(IBuilder<FormatterRegistry> registry);

}
