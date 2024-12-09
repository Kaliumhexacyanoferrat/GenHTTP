using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using System.Globalization;

namespace GenHTTP.Modules.I18n.Provider;

public sealed class LocalizationConcern : IConcern
{
    #region Get-/Setters

    public IHandler Content { get; }

    private readonly CultureInfo _defaultCulture;
    private readonly CultureSelectorCombinedAsyncDelegate _cultureSelector;
    private readonly CultureFilterAsyncDelegate _cultureFilter;
    private readonly CultureSetterAsyncDelegate _cultureSetter;

    #endregion

    #region Initialization

    public LocalizationConcern(
        IHandler content,
        CultureInfo defaultCulture,
        CultureSelectorCombinedAsyncDelegate cultureSelector,
        CultureFilterAsyncDelegate cultureFilter,
        CultureSetterAsyncDelegate cultureSetter
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

    private async ValueTask<CultureInfo?> ResolveCultureInfoAsync(IRequest request)
    {
        await foreach (var candidate in _cultureSelector(request))
        {
            if (await _cultureFilter(request, candidate))
            {
                return candidate;
            }
        }
        return null;
    }

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var culture = await ResolveCultureInfoAsync(request) ?? _defaultCulture;

        await _cultureSetter(request, culture);

        return await Content.HandleAsync(request);
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    #endregion
}
