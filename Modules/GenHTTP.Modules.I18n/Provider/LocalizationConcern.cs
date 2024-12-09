using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using System.Globalization;

namespace GenHTTP.Modules.I18n.Provider;

public sealed class LocalizationConcern : IConcern
{
    #region Get-/Setters

    public IHandler Content { get; }

    private readonly CultureInfo _defaultCulture;
    private readonly CultureSelectorDelegate _cultureSelector;
    private readonly CultureFilterDelegate _cultureFilter;
    private readonly CultureSetterDelegate _cultureSetter;

    #endregion

    #region Initialization

    public LocalizationConcern(
        IHandler content,
        CultureInfo defaultCulture,
        CultureSelectorDelegate cultureSelector,
        CultureFilterDelegate cultureFilter,
        CultureSetterDelegate cultureSetter
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
