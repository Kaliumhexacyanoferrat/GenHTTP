using System.Globalization;

namespace GenHTTP.Modules.I18n;

public static class Localization
{
    #region Builder

    /// <summary>
    /// Creates a localization handler that parses and sets 
    /// <see cref="CultureInfo"/> based on defined rules.
    /// </summary>
    /// <returns>By default culture is read from request header 
    /// and is set to <see cref=" CultureInfo.CurrentUICulture"/>.</returns>
    public static LocalizationConcernBuilder Create() => new();

    #endregion
}
