using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.I18n.Parsers;
using System.Globalization;

namespace GenHTTP.Modules.I18n;

/// <summary>
/// Builder class to configure and create an instance of <see cref="LocalizationConcern"/>.
/// </summary>
public sealed class LocalizationConcernBuilder : IConcernBuilder
{
    #region Fields

    private CultureInfo _defaultCulture = CultureInfo.CurrentCulture;

    private readonly List<CultureSelector_Delegate> _cultureSelectors = [];
    private CultureFilter_Delegate _cultureFilter = (_, _) => true;
    private readonly List<CultureSetter_Delegate> _cultureSetters = [];

    #endregion

    #region Functionality

    #region Selectors

    public LocalizationConcernBuilder AddFromCookie(string cookieName = "lang")
        => AddFromLanguage(request =>
        {
            request.Cookies.TryGetValue(cookieName, out var languageCookie);
            return languageCookie.Value;
        });

    public LocalizationConcernBuilder AddFromQuery(string queryName = "lang")
        => AddFromLanguage(request =>
        {
            request.Query.TryGetValue(queryName, out var language);
            return language;
        });

    public LocalizationConcernBuilder AddFromHeader(string headerName = "Accept-Language")
        => AddFromLanguage(request =>
        {
            request.Headers.TryGetValue(headerName, out var language);
            return language;
        });

    public LocalizationConcernBuilder AddFromLanguage(Func<IRequest, string?> languageSelector)
        => AddFromRequest(request =>
        {
            var language = languageSelector(request);
            return CultureInfoParser.ParseFromLanguage(language);
        });

    public LocalizationConcernBuilder AddFromRequest(CultureSelector_Delegate cultureSelector)
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

    public LocalizationConcernBuilder Supports(CultureFilter_Delegate cultureFilter)
    {
        _cultureFilter = cultureFilter;
        return this;
    }

    #endregion

    #region Setters

    public LocalizationConcernBuilder AddSet(bool currentCulture = false, bool currentUICulture = true)
    {
        //Note: this is a minor optimization so that the flags are not evaluated for each request
        if (currentCulture)
        {
            AddSet(culture => CultureInfo.CurrentCulture = culture);
        }
        if (currentUICulture)
        {
            AddSet(culture => CultureInfo.CurrentUICulture = culture);
        }
        return this;
    }

    public LocalizationConcernBuilder AddSet(Action<CultureInfo> cultureSetter)
        => AddSet((_, culture) => cultureSetter(culture));

    public LocalizationConcernBuilder AddSet(CultureSetter_Delegate cultureSetter)
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
            AddFromHeader();
        }

        if (_cultureSetters.Count == 0)
        {
            AddSet();
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

    private IEnumerable<CultureInfo> CultureSelector(IRequest request)
        => _cultureSelectors.SelectMany(selector => selector(request));   

    private void CultureSetter(IRequest request, CultureInfo cultureInfo)
    {
        foreach (var setter in _cultureSetters)
        {
            setter(request, cultureInfo);
        }
    }

    #endregion
}
