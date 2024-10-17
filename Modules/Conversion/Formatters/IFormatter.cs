namespace GenHTTP.Modules.Conversion.Formatters;

/// <summary>
/// Allows to add support for a specific type to be used as a parameter
/// of a web service or controller as well as in form encoded data.
/// </summary>
public interface IFormatter
{

    /// <summary>
    /// Checks, whether the formatter is capable of handling the given type.
    /// </summary>
    /// <param name="type">The type to be checked</param>
    /// <returns>true, if the formatter can be used to read and write such types</returns>
    bool CanHandle(Type type);

    /// <summary>
    /// Converts the given string into the specified type.
    /// </summary>
    /// <param name="value">The value to be converted</param>
    /// <param name="type">The type to convert the value to</param>
    /// <returns>The value converted into the given type</returns>
    /// <remarks>
    /// Used by the framework to read parameters to be passed to controllers or the like.
    /// </remarks>
    object? Read(string value, Type type);

    /// <summary>
    /// Converts the given object instance of the given type into a string representation.
    /// </summary>
    /// <param name="value">The value to be formatted</param>
    /// <param name="type">The declared type of the value</param>
    /// <returns>The string representation of the given value</returns>
    /// <remarks>
    /// Used by the framework to serialize a single value into the response's body
    /// or to generate form encoded data.
    /// </remarks>
    string? Write(object value, Type type);
}
