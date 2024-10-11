using System;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Modules.Conversion;

public static class Extensions
{

    /// <summary>
    /// Attempts to convert the given string value to the specified type.
    /// </summary>
    /// <param name="value">The value to be converted</param>
    /// <param name="type">The target type to convert the value to</param>
    /// <param name="formatters">The formatting to be used to actually perform the conversion</param>
    /// <returns>The converted value</returns>
    public static object? ConvertTo(this string? value, Type type, FormatterRegistry formatters)
    {
            if (string.IsNullOrEmpty(value))
            {
                if (Nullable.GetUnderlyingType(type) is not null)
                {
                    return null;
                }
                else if (type == typeof(string))
                {
                    return value;
                }
                else
                {
                    return Activator.CreateInstance(type);
                }
            }

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
