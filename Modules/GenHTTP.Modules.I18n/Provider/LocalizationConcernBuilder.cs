using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.I18n.Parsers;
using System.Globalization;

namespace GenHTTP.Modules.I18n.Provider;

/// <summary>
/// Builder class to configure and create an instance of <see cref="LocalizationConcern"/>.
/// </summary>
public sealed class LocalizationConcernBuilder : IConcernBuilder
{
    #region Fields

    private CultureInfo _defaultCulture = CultureInfo.CurrentUICulture;

    private readonly List<CultureSelectorAsyncDelegate> _cultureSelectors = [];
    private CultureFilterAsyncDelegate _cultureFilter = (_, _) => ValueTask.FromResult(true);
    private readonly List<AsyncOrSyncSetter> _cultureSetters = [];

    #endregion

    #region Functionality

    #region Selectors

    /// <summary>
    /// Extracts the language from a cookie.
    /// </summary>
    /// <param name="cookieName">The name of the cookie to extract the language from.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder FromCookie(string cookieName = "lang")
        => FromRequest(request =>
        {
            request.Cookies.TryGetValue(cookieName, out var languageCookie);
            return languageCookie.Value;
        });

    /// <summary>
    /// Extracts the language from a query parameter.
    /// </summary>
    /// <param name="queryName">The name of the query parameter to extract the language from.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder FromQuery(string queryName = "lang")
        => FromRequest(request =>
        {
            request.Query.TryGetValue(queryName, out var language);
            return language;
        });

    /// <summary>
    /// Extracts the language from a header.
    /// </summary>
    /// <param name="headerName">The name of the header to extract the language from.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder FromHeader(string headerName = "Accept-Language")
        => FromRequest(request =>
        {
            request.Headers.TryGetValue(headerName, out var language);
            return language;
        });

    /// <summary>
    /// Extracts the language from a custom selector.
    /// </summary>
    /// <param name="languageSelector">The selector to extract the language from.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder FromRequest(Func<IRequest, string?> languageSelector)
        => FromRequest(request => ValueTask.FromResult(languageSelector(request)));

    /// <summary>
    /// Extracts the language from a custom async selector.
    /// </summary>
    /// <param name="languageSelector">The async selector to extract the language from.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder FromRequest(Func<IRequest, ValueTask<string?>> languageSelector)
        => FromRequest(async request =>
        {
            var language = await languageSelector(request);
            return CultureInfoParser.ParseFromLanguage(language);
        });

    /// <summary>
    /// Extracts the culture from a custom selector.
    /// </summary>
    /// <param name="cultureSelector">The selector to extract the culture from.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder FromRequest(CultureSelectorDelegate cultureSelector)
        => FromRequest(request => ValueTask.FromResult(cultureSelector(request)));

    /// <summary>
    /// Extracts the culture from a custom async selector.
    /// </summary>
    /// <param name="cultureSelector">The async selector to extract the culture from.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder FromRequest(CultureSelectorAsyncDelegate cultureSelector)
    {
        _cultureSelectors.Add(cultureSelector);
        return this;
    }

    #endregion

    #region Filters

    /// <summary>
    /// Sets the supported cultures.
    /// </summary>
    /// <param name="supportedCultures">The supported cultures.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder Supports(params CultureInfo[] supportedCultures)
    {
        var closure = supportedCultures.ToHashSet();
        return Supports(closure.Contains);
    }

    /// <summary>
    /// Sets a custom filter of supported cultures.
    /// </summary>
    /// <param name="culturePredicate">The predicate to filter the cultures.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder Supports(Predicate<CultureInfo> culturePredicate)
        => Supports((_, culture) => culturePredicate(culture));

    /// <summary>
    /// Sets a custom async filter of supported cultures.
    /// </summary>
    /// <param name="culturePredicate">The async predicate to filter the cultures.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder Supports(Func<CultureInfo, ValueTask<bool>> culturePredicate)
        => Supports((_, culture) => culturePredicate(culture));

    /// <summary>
    /// Sets a custom filter (using <see cref="IRequest"/>) of supported cultures.
    /// </summary>
    /// <param name="cultureFilter">The filter to filter the cultures.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder Supports(CultureFilterDelegate cultureFilter)
        => Supports((request, culture) => ValueTask.FromResult(cultureFilter(request, culture)));

    /// <summary>
    /// Sets a custom async filter (using <see cref="IRequest"/>) of supported cultures.
    /// </summary>
    /// <param name="cultureFilter">The async filter to filter the cultures.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder Supports(CultureFilterAsyncDelegate cultureFilter)
    {
        _cultureFilter = cultureFilter;
        return this;
    }

    #endregion

    #region Setters

    /// <summary>
    /// Sets the current culture and UI culture.
    /// </summary>
    /// <param name="currentCulture">Whether to set the current culture.</param>
    /// <param name="currentUICulture">Whether to set the current UI culture.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown when both flags are set to false.</exception>
    public LocalizationConcernBuilder Setter(bool currentCulture = false, bool currentUICulture = true)
    {
        if (!currentCulture && !currentUICulture)
        {
            throw new ArgumentException("At least one of the flags must be set to true.", nameof(currentCulture));
        }

        //Note: this is a minor optimization so that the flags are not evaluated for each request
        if (currentCulture)
        {
            Setter(culture => CultureInfo.CurrentCulture = culture);
        }
        if (currentUICulture)
        {
            Setter(culture => CultureInfo.CurrentUICulture = culture);
        }
        return this;
    }

    /// <summary>
    /// Sets a custom async culture setter.
    /// </summary>
    /// <param name="cultureSetter">The async culture setter.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder Setter(Func<CultureInfo, ValueTask> cultureSetter)
        => Setter((_, culture) => cultureSetter(culture));

    /// <summary>
    /// Sets a custom culture setter.
    /// </summary>
    /// <param name="cultureSetter">The culture setter.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder Setter(Action<CultureInfo> cultureSetter)
        => Setter((_, culture) => cultureSetter(culture));

    /// <summary>
    /// Sets a custom culture setter (using <see cref="IRequest"/>).
    /// </summary>
    /// <param name="cultureSetter">The culture setter.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder Setter(CultureSetterDelegate cultureSetter)
    {
        _cultureSetters.Add(new(SyncSetter: cultureSetter));
        return this;
    }

    /// <summary>
    /// Sets a custom async culture setter (using <see cref="IRequest"/>).
    /// </summary>
    /// <param name="cultureSetter">The async culture setter.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder Setter(CultureSetterAsyncDelegate cultureSetter)
    {
        _cultureSetters.Add(new (AsyncSetter: cultureSetter));
        return this;
    }

    #endregion

    #region Default

    /// <summary>
    /// Sets the default culture that is used as a fallback.
    /// </summary>
    /// <param name="culture">The default culture.</param>
    /// <returns></returns>
    public LocalizationConcernBuilder Default(CultureInfo culture)
    {
        _defaultCulture = culture;
        return this;
    }

    #endregion

    public IConcern Build(IHandler content)
    {
        if (_cultureSelectors.Count == 0)
        {
            FromHeader();
        }

        if (_cultureSetters.Count == 0)
        {
            Setter();
        }

        return new LocalizationConcern(
            content,
            _defaultCulture,
            CultureSelector,
            _cultureFilter,
            [.. _cultureSetters]
            );
    }

    #endregion

    #region Composite functions

    private async IAsyncEnumerable<CultureInfo> CultureSelector(IRequest request)
    {
        foreach (var selector in _cultureSelectors)
        {
            var cultures = await selector(request);
            foreach (var culture in cultures)
            {
                yield return culture;
            }
        }
    }

    #endregion
}
