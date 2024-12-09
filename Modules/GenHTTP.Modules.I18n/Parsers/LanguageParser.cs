using System.Buffers;
using System.Globalization;

namespace GenHTTP.Modules.I18n.Parsers;

public static class CultureInfoParser
{
    private static readonly Comparer<(string Language, double Quality)> QualityComparer =
        Comparer<(string Language, double Quality)>.Create((a, b) => b.Quality.CompareTo(a.Quality));

    /// <summary>
    /// Parses the given language header (e.g. from Accept-Language) into an array of CultureInfo,
    /// sorted by their quality values in descending order. If no valid languages are found,
    /// returns an empty array.
    /// 
    /// Specification: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Language
    /// 
    /// This implementation uses ArrayPool to minimize allocations.
    /// </summary>
    /// <param name="language">The language header string to parse.</param>
    /// <returns>An array of <see cref="CultureInfo"/> objects parsed from the input string.</returns>
    public static CultureInfo[] ParseFromLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return [];
        }

        var span = language.AsSpan().Trim();
        if (span.IsEmpty)
        {
            return [];
        }

        // Count how many segments (comma-delimited)
        var count = 1;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == ',') count++;
        }

        var pool = ArrayPool<(string Language, double Quality)>.Shared;
        (string Language, double Quality)[] rentedArray = pool.Rent(count);

        try
        {

            var actualCount = 0;
            var start = 0;
            int commaIndex;
            do
            {
                commaIndex = span[start..].IndexOf(',');
                ReadOnlySpan<char> segment;
                if (commaIndex >= 0)
                {
                    segment = span.Slice(start, commaIndex).Trim();
                    start += commaIndex + 1;
                }
                else
                {
                    segment = span[start..].Trim();
                }

                if (!segment.IsEmpty)
                {
                    // segment format: "lang[-region][;q=...]" or just "lang[-region]"
                    var semicolonIndex = segment.IndexOf(';');
                    var qValue = 1d;
                    ReadOnlySpan<char> languagePart;

                    if (semicolonIndex >= 0)
                    {
                        languagePart = segment[..semicolonIndex].Trim();
                        var afterSemicolon = segment[(semicolonIndex + 1)..].Trim();

                        // afterSemicolon should look like "q=0.5"
                        if (afterSemicolon.StartsWith("q=", StringComparison.OrdinalIgnoreCase))
                        {
                            var qSpan = afterSemicolon[2..].Trim(); // skip 'q='
                            if (!double.TryParse(qSpan, NumberStyles.Number, CultureInfo.InvariantCulture, out qValue))
                            {
                                qValue = 1;
                            }
                        }
                    }
                    else
                    {
                        languagePart = segment;
                    }

                    if (!languagePart.IsEmpty && actualCount < rentedArray.Length)
                    {
                        // Convert languagePart to string only once here
                        rentedArray[actualCount++] = (languagePart.ToString(), qValue);
                    }
                }

            } while (commaIndex >= 0);

            if (actualCount == 0)
            {
                return [];
            }

            // Sort by quality descending
            Array.Sort(rentedArray, 0, actualCount, QualityComparer);

            // Convert to CultureInfo array and return rented array
            List<CultureInfo> results = new(actualCount);

            for (int i = 0; i < actualCount; i++)
            {
                var (lang, _) = rentedArray[i];
                if (string.IsNullOrWhiteSpace(lang))
                {
                    continue;
                }

                try
                {
                    results.Add(CultureInfo.CreateSpecificCulture(lang));
                }
                catch (CultureNotFoundException)
                {
                    // skip invalid cultures
                }
            }

            return results.Count > 0 ? [.. results] : [];
        }
        finally
        {
            pool.Return(rentedArray);
        }
    }
}
