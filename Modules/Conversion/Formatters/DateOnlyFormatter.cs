using System.Text.RegularExpressions;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Formatters;

public sealed partial class DateOnlyFormatter : IFormatter
{
    private static readonly Regex DateOnlyPattern = CreateDateOnlyPattern();

    public bool CanHandle(Type type) => type == typeof(DateOnly);

    public object Read(string value, Type type)
    {
        var match = DateOnlyPattern.Match(value);

        if (match.Success)
        {
            var year = int.Parse(match.Groups[1].Value);
            var month = int.Parse(match.Groups[2].Value);
            var day = int.Parse(match.Groups[3].Value);

            return new DateOnly(year, month, day);
        }

        throw new ProviderException(ResponseStatus.BadRequest, $"Input does not match the requested format (yyyy-mm-dd): {value}");
    }

    public string Write(object value, Type type) => ((DateOnly)value).ToString("yyyy-MM-dd");

    [GeneratedRegex(@"^([0-9]{4})\-([0-9]{2})\-([0-9]{2})$", RegexOptions.Compiled)]
    private static partial Regex CreateDateOnlyPattern();
    
}
