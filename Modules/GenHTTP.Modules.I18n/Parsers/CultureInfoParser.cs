using System.Globalization;

namespace GenHTTP.Modules.I18n.Parsers;

public static class CultureInfoParser
{
    public static CultureInfo[] ParseFromLanguage(string? language)
        => Parse(LanguageParser.ParseAcceptLanguageHeader(language));

    public static CultureInfo[] Parse(IEnumerable<string> languages)
        => languages
            .Select(Parse)
            .OfType<CultureInfo>()
            .ToArray();

    private static CultureInfo? Parse(string name)
    {
        try
        {
            return CultureInfo.CreateSpecificCulture(name);
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }
}
