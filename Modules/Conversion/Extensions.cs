using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Modules.Conversion;

public static class Extensions
{

    /// <summary>
    /// Attempts to convert the given memory value to the specified type.
    /// </summary>
    /// <param name="value">The value to be converted</param>
    /// <param name="type">The target type to convert the value to</param>
    /// <param name="formatters">The formatting to be used to actually perform the conversion</param>
    /// <returns>The converted value</returns>
    public static object? ConvertTo(this ReadOnlyMemory<byte>? value, Type type, FormatterRegistry formatters)
    {
        if (value is null || value.Value.IsEmpty)
        {
            if (Nullable.GetUnderlyingType(type) is not null)
            {
                return null;
            }

            if (type == typeof(string))
            {
                return null;
            }

            return Activator.CreateInstance(type);
        }

        return value.Value.ConvertTo(type, formatters);
    }

    public static object? ConvertTo(this ReadOnlyMemory<byte> value, Type type, FormatterRegistry formatters)
    {
        try
        {
            var actualType = Nullable.GetUnderlyingType(type) ?? type;

            return formatters.Read(value, actualType);
        }
        catch (Exception e)
        {
            throw new ProviderException(ResponseStatus.BadRequest, $"Unable to convert value '{value}' to type '{type}'", e);
        }
    }
    
}
