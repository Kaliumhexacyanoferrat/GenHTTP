using System;
using System.Text.RegularExpressions;

namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class DateOnlyFormatter : IFormatter
{
    private static readonly Regex DATE_ONLY_PATTERN = new(@"^([0-9]{4})\-([0-9]{2})\-([0-9]{2})$", RegexOptions.Compiled);

    public bool CanHandle(Type type) => type == typeof(DateOnly);

    public object? Read(string value, Type type)
    {
            var match = DATE_ONLY_PATTERN.Match(value);

            if (match.Success)
            {
                var year = int.Parse(match.Groups[1].Value);
                var month = int.Parse(match.Groups[2].Value);
                var day = int.Parse(match.Groups[3].Value);

                return new DateOnly(year, month, day);
            }

            throw new ArgumentException($"Input does not match the requested format (yyyy-mm-dd): {value}");
        }

    public string? Write(object value, Type type) => ((DateOnly)value).ToString("yyyy-MM-dd");

}
