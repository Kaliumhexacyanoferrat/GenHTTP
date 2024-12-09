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

    private CultureInfo _defaultCulture = CultureInfo.CurrentCulture;

    private readonly List<CultureSelectorAsyncDelegate> _cultureSelectors = [];
    private CultureFilterAsyncDelegate _cultureFilter = (_, _) => ValueTask.FromResult(true);
    private readonly List<CultureSetterAsyncDelegate> _cultureSetters = [];

    #endregion

    #region Functionality

    #region Selectors

    public LocalizationConcernBuilder FromCookie(string cookieName = "lang")
        => FromLanguage(request =>
        {
            request.Cookies.TryGetValue(cookieName, out var languageCookie);
            return languageCookie.Value;
        });

    public LocalizationConcernBuilder FromQuery(string queryName = "lang")
        => FromLanguage(request =>
        {
            request.Query.TryGetValue(queryName, out var language);
            return language;
        });

    public LocalizationConcernBuilder FromHeader(string headerName = "Accept-Language")
        => FromLanguage(request =>
        {
            request.Headers.TryGetValue(headerName, out var language);
            return language;
        });

    public LocalizationConcernBuilder FromLanguage(Func<IRequest, string?> languageSelector)
        => FromLanguage(request => ValueTask.FromResult(languageSelector(request)));

    public LocalizationConcernBuilder FromLanguage(Func<IRequest, ValueTask<string?>> languageSelector)
        => FromRequest(async request =>
        {
            var language = await languageSelector(request);
            return CultureInfoParser.ParseFromLanguage(language);
        });

    public LocalizationConcernBuilder FromRequest(CultureSelectorDelegate cultureSelector)        
        => FromRequest(request => ValueTask.FromResult(cultureSelector(request)));

    public LocalizationConcernBuilder FromRequest(CultureSelectorAsyncDelegate cultureSelector)
    {
        _cultureSelectors.Add(cultureSelector);
        return this;
    }

    #endregion

    #region Filters

    public LocalizationConcernBuilder Supports(params CultureInfo[] supportedCultures)
    {
        var closure = supportedCultures.ToHashSet();
        return Supports(closure.Contains);
    }

    public LocalizationConcernBuilder Supports(Predicate<CultureInfo> culturePredicate)
        => Supports((_, culture) => culturePredicate(culture));

    public LocalizationConcernBuilder Supports(Func<CultureInfo, ValueTask<bool>> culturePredicate)
        => Supports((_, culture) => culturePredicate(culture));

    public LocalizationConcernBuilder Supports(CultureFilterDelegate cultureFilter)
        => Supports((request, culture) => ValueTask.FromResult(cultureFilter(request, culture)));

    public LocalizationConcernBuilder Supports(CultureFilterAsyncDelegate cultureFilter)
    {
        _cultureFilter = cultureFilter;
        return this;
    }

    #endregion

    #region Setters

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

    public LocalizationConcernBuilder Setter(Func<CultureInfo, ValueTask> cultureSetter)
        => Setter((_, culture) => cultureSetter(culture));

    public LocalizationConcernBuilder Setter(Action<CultureInfo> cultureSetter)
        => Setter((_, culture) => cultureSetter(culture));

    public LocalizationConcernBuilder Setter(CultureSetterDelegate cultureSetter)
        => Setter((request, culture) => 
        {
            cultureSetter(request, culture);
            return ValueTask.CompletedTask;
        });

    public LocalizationConcernBuilder Setter(CultureSetterAsyncDelegate cultureSetter)
    {
        _cultureSetters.Add(cultureSetter);
        return this;
    }

    #endregion

    #region Default

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
            CultureSetter
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


    private async ValueTask CultureSetter(IRequest request, CultureInfo cultureInfo)
    {
        foreach (var setter in _cultureSetters)
        {
            await setter(request, cultureInfo);
        }
    }

    #endregion
}
