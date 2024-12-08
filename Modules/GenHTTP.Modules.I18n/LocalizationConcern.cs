using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using System.Globalization;

namespace GenHTTP.Modules.I18n;

public sealed class LocalizationConcern : IConcern
{
    #region Get-/Setters

    public IHandler Content { get; }

    private const string LangKey = "lang";
    private readonly CultureInfo _defaultCulture;
    private readonly CultureSelector_Delegate _cultureSelector;
    private readonly CultureFilter_Delegate _cultureFilter;
    private readonly CultureSetter_Delegate _cultureSetter;

    #endregion

    #region Initialization

    public LocalizationConcern(
        IHandler content,
        CultureInfo defaultCulture,
        CultureSelector_Delegate cultureSelector,
        CultureFilter_Delegate cultureFilter,
        CultureSetter_Delegate cultureSetter
        )
    {
        Content = content;

        _defaultCulture = defaultCulture;
        _cultureSelector = cultureSelector;        
        _cultureFilter = cultureFilter;

        _cultureSetter = cultureSetter;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var culture = (_cultureSelector(request) ?? [])
            .FirstOrDefault(c => _cultureFilter(request, c))
            ?? _defaultCulture;

        _cultureSetter(request, culture);

        return await Content.HandleAsync(request);
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    #endregion
}
