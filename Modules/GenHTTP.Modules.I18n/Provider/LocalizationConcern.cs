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
    private readonly AsyncOrSyncSetter[] _cultureSetters;

    #endregion

    #region Initialization

    public LocalizationConcern(
        IHandler content,
        CultureInfo defaultCulture,
        CultureSelectorCombinedAsyncDelegate cultureSelector,
        CultureFilterAsyncDelegate cultureFilter,
        AsyncOrSyncSetter[] cultureSetters
        )
    {
        Content = content;

        _defaultCulture = defaultCulture;
        _cultureSelector = cultureSelector;
        _cultureFilter = cultureFilter;

        _cultureSetters = cultureSetters;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var culture = await ResolveCultureInfoAsync(request) ?? _defaultCulture;

        foreach(var _cultureSetter in _cultureSetters)
        {
            _cultureSetter.SyncSetter?.Invoke(request, culture);

            if (_cultureSetter.AsyncSetter != null)
            {
                await _cultureSetter.AsyncSetter(request, culture);
            }
        }

        var response = await Content.HandleAsync(request);

        if (response?.Content != null)
        {
            var tag = culture.IetfLanguageTag;

            if (!string.IsNullOrEmpty(tag))
            {
                response.Headers.TryAdd("Content-Language", tag);
            }
        }

        return response;
    }

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

    #endregion

}
