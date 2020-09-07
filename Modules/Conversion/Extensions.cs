using System;
using System.Globalization;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion
{
    
    public static class Extensions
    {

        /// <summary>
        /// Attempts to convert the given string value to the specified type.
        /// </summary>
        /// <param name="value">The value to be converted</param>
        /// <param name="type">The target type to convert the value to</param>
        /// <returns>The converted value</returns>
        public static object? ConvertTo(this string? value, Type type)
        {
            if (string.IsNullOrEmpty(value) && Nullable.GetUnderlyingType(type) != null)
            {
                return null;
            }

            try
            {
                var actualType = Nullable.GetUnderlyingType(type) ?? type;

                if (actualType.IsEnum)
                {
                    return Enum.Parse(actualType, value);
                }

                if (actualType == typeof(bool))
                {
                    if (value == "1" || value == "on") return true;
                    else if (value == "0" || value == "off") return false;
                }

                return Convert.ChangeType(value, actualType, CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                throw new ProviderException(ResponseStatus.BadRequest, $"Unable to convert value '{value}' to type '{type}'", e);
            }
        }

    }

}
