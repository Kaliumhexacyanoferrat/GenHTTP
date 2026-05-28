using System.Text;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Caching;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using StreamContent = GenHTTP.Modules.IO.Streaming.StreamContent;

namespace GenHTTP.Modules.ServerCaching.Provider;

public sealed class ServerCacheHandler : IConcern
{

    #region Get-/Setters

    public IHandler Content { get; }

    private ICache<CachedResponse> Meta { get; }

    private ICache<Stream> Data { get; }

    private bool Invalidate { get; }

    private Func<IRequest, IResponse, bool>? Predicate { get; }

    #endregion

    #region Initialization

    public ServerCacheHandler(IHandler content,
        ICache<CachedResponse> meta, ICache<Stream> data,
        Func<IRequest, IResponse, bool>? predicate, bool invalidate)
    {
        Content = content;

        Meta = meta;
        Data = data;

        Predicate = predicate;
        Invalidate = invalidate;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (request.HasType(RequestMethod.Get, RequestMethod.Head))
        {
            var response = Invalidate ? await Content.HandleAsync(request) : null;

            var key = request.GetKey();

            if (TryFindMatching(await Meta.GetEntriesAsync(key), request, out var match))
            {
                if (match != null)
                {
                    if (!Invalidate || !await CheckChangedAsync(response!, match))
                    {
                        var content = await Data.GetEntryAsync(key, match.Variations.GetVariationKey());

                        return GetResponse(request, match, content);
                    }
                }
            }

            if (!Invalidate)
            {
                response = await Content.HandleAsync(request);
            }

            var status = response?.Status;

            if (response != null && (status == ResponseStatus.Ok || status == ResponseStatus.NoContent))
            {
                if (Predicate == null || Predicate(request, response))
                {
                    var cached = await GetResponse(response, request);

                    var variationKey = cached.Variations.GetVariationKey();

                    await Meta.StoreAsync(key, variationKey, cached);

                    if (response.Content != null)
                    {
                        await Data.StoreDirectAsync(key, variationKey, target => response.Content.WriteAsync(new StreamSink(target)));
                    }
                    else
                    {
                        await Data.StoreAsync(key, variationKey, null);
                    }
                }
            }

            return response;
        }

        return await Content.HandleAsync(request);
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    private static bool TryFindMatching(CachedResponse[] list, IRequest request, out CachedResponse? response)
    {
        response = null;

        foreach (var variant in list)
        {
            if (variant.Variations != null)
            {
                foreach (var header in variant.Variations)
                {
                    var actual = request.Header.Headers.GetEntry(header.Key);

                    if (actual != null)
                    {
                        if (header.Value == actual)
                        {
                            response = variant;
                            return true;
                        }
                    }
                }
            }
            else
            {
                response = variant;
            }
        }

        return response != null;
    }

    private static IResponse GetResponse(IRequest request, CachedResponse cached, Stream? content)
    {
        var response = request.Respond()
                              .Status((ResponseStatus)cached.StatusCode); // todo

        /*if (cached.Cookies != null)
        {
            foreach (var cookie in cached.Cookies)
            {
                response.Cookie(cookie.Value);
            }
        }*/

        foreach (var header in cached.Headers)
        {
            response.Header(header.Key, header.Value);
        }

        if (content != null)
        {
            ContentType? type = null;

            if (cached.ContentType != null)
            {
                type = new(cached.ContentType);
            }

            ReadOnlyMemory<byte>? encoding = null;

            if (cached.ContentEncoding != null)
            {
                encoding = Encoding.ASCII.GetBytes(cached.ContentEncoding);
            }

            response.Content(new StreamContent(content, type ?? ContentType.ApplicationOctetStream, cached.ContentLength, encoding, () => new ValueTask<ulong?>(cached.ContentChecksum)));
        }

        return response.Build();
    }

    private static async ValueTask<CachedResponse> GetResponse(IResponse response, IRequest request)
    {
        var headers = new Dictionary<string, string>();

        var responseHeaders = response.Headers;

        string? vary = null;

        for (var i = 0; i < responseHeaders.Count; i++)
        {
            var header = responseHeaders[i];

            var key = Encoding.ASCII.GetString(header.Key.Span);
            var value = Encoding.ASCII.GetString(header.Value.Span);

            if (key == "Vary")
            {
                vary = value;
            }

            headers.Add(key, value);
        }

        var cookies = new Dictionary<string, Cookie>(); // todo

        Dictionary<string, string>? variations = null;

        if (vary != null)
        {
            variations = new Dictionary<string, string>();

            foreach (var entry in vary.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var value = request.Header.Headers.GetEntry(entry);

                if (value != null)
                {
                    variations.Add(entry, value);
                }
            }
        }

        var (checksum, contentLength, contentType, contentEncoding) = await ExtractContentValues(response);

        return new CachedResponse((int)response.Status, variations, headers, cookies, contentType, contentEncoding, contentLength, checksum);
    }

    private static async ValueTask<bool> CheckChangedAsync(IResponse current, CachedResponse cached)
    {
        var (checksum, contentLength, contentType, contentEncoding) = await ExtractContentValues(current);

        return cached.ContentChecksum != checksum
            || cached.ContentEncoding != contentEncoding
            || cached.ContentLength != contentLength
            || cached.ContentType != contentType
            || HeadersChanged(current, cached);
        // todo:   || current.Cookies.Count != cached.Cookies?.Count || current.Cookies.Except(cached.Cookies).Any();
    }

    private static async ValueTask<(ulong?, ulong?, string?, string?)> ExtractContentValues(IResponse response)
    {
        var content = response.Content;

        if (content != null)
        {
            var checksum = await content.CalculateChecksumAsync();

            var contentLength = content.Length;
            var contentType = (content.Type != null) ? Encoding.ASCII.GetString(content.Type.Value.Value.Span) : null;
            var contentEncoding = (content.Encoding != null) ? Encoding.ASCII.GetString(content.Encoding.Value.Span) : null;

            return (checksum, contentLength, contentType, contentEncoding);
        }

        return default;
    }

    private static bool HeadersChanged(IResponse current, CachedResponse cached)
    {
        var headers = current.Headers;

        for (var i = 0; i < headers.Count; i++)
        {
            var header = headers[i];

            var key = Encoding.ASCII.GetString(header.Key.Span);
            var value = Encoding.ASCII.GetString(header.Value.Span);

            if (cached.Headers.TryGetValue(key, out var cachedValue))
            {
                if (cachedValue != value)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    #endregion

}
