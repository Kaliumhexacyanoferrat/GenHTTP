namespace GenHTTP.Modules.I18n.Parsers;

public static class LanguageParser
{
    /// <summary>
    /// Parse the Accept-Language header into an enumerable of language tags.
    /// Specification: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Language
    /// </summary>
    /// <param name="acceptLanguageHeader"></param>
    /// <returns></returns>
    public static IEnumerable<string> ParseAcceptLanguageHeader(string? acceptLanguageHeader)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguageHeader))
        {
            return [];
        }

        return acceptLanguageHeader
            .Split(',')
            .Select(lang =>
            {
                var parts = lang.Split(';');
                var language = parts[0].Trim();
                var quality = parts.Length > 1 && parts[1].StartsWith("q=")
                    ? double.TryParse(parts[1][2..].Trim(), out var qValue) ? qValue : 1.0
                    : 1.0;
                return new { Language = language, Quality = quality };
            })
            .OrderByDescending(lang => lang.Quality)
            .Select(lang => lang.Language)
            .Where(lang => !string.IsNullOrWhiteSpace(lang));
    }
}
