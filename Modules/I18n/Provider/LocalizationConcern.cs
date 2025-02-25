using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using System.Globalization;

namespace GenHTTP.Modules.I18n.Provider;

public sealed class LocalizationConcern : IConcern
{
    #region Get-/Setters

    public IHandler Content { get; }

    private readonly CultureInfo _DefaultCulture;
    private readonly CultureSelectorCombinedAsyncDelegate _CultureSelector;
    private readonly CultureFilterAsyncDelegate _CultureFilter;
    private readonly AsyncOrSyncSetter[] _CultureSetters;

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

        _DefaultCulture = defaultCulture;
        _CultureSelector = cultureSelector;
        _CultureFilter = cultureFilter;

        _CultureSetters = cultureSetters;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var culture = await ResolveCultureInfoAsync(request) ?? _DefaultCulture;

        foreach(var _cultureSetter in _CultureSetters)
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
        await foreach (var candidate in _CultureSelector(request))
        {
            if (await _CultureFilter(request, candidate))
            {
                return candidate;
            }
        }
        return null;
    }

    #endregion

}
