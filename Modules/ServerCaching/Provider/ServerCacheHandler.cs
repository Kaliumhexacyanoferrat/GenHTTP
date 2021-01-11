using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Caching;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.ServerCaching.Provider
{

    public sealed class ServerCacheHandler : IConcern
    {

        #region Get-/Setters

        public IHandler Content { get; }

        public IHandler Parent { get; }

        private ICache<CachedResponse> Meta { get; }

        private ICache<Stream> Data { get; }

        #endregion

        #region Initialization

        public ServerCacheHandler(IHandler parent, Func<IHandler, IHandler> contentFactory, ICache<CachedResponse> meta, ICache<Stream> data)
        {
            Parent = parent;
            Content = contentFactory(this);

            Meta = meta;
            Data = data;
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            if (request.HasType(RequestMethod.GET, RequestMethod.HEAD))
            {
                var key = request.GetKey();

                if (TryFindMatching(await Meta.GetEntriesAsync(key), request, out var match))
                {
                    if (match != null)
                    {
                        var content = await Data.GetEntryAsync(key, match.Variations.GetVariationKey());

                        return GetResponse(request, match, content);
                    }
                }

                var response = await Content.HandleAsync(request);

                if (response != null)
                {
                    var cached = GetResponse(response);

                    var variationKey = cached.Variations.GetVariationKey();

                    await Meta.StoreAsync(key, variationKey, cached);

                    if (response.Content != null)
                    {
                        await Data.StoreDirectAsync(key, variationKey, (target) => response.Content.WriteAsync(target, 4096));
                    }
                    else
                    {
                        await Data.StoreAsync(key, variationKey, null);
                    }
                }

                return response;
            }

            return await Content.HandleAsync(request);
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => Content.GetContent(request);

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
                        if (variant.Headers.TryGetValue(header, out var expected))
                        {
                            if (request.Headers.TryGetValue(header, out var actual))
                            {
                                if (expected == actual)
                                {
                                    response = variant;
                                    return true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    response = variant;
                }
            }

            return (response != null);
        }

        private static IResponse GetResponse(IRequest request, CachedResponse cached, Stream? content)
        {
            var response = request.Respond()
                                  .Status(cached.Status.RawStatus, cached.Status.Phrase);

            if (cached.ContentEncoding != null)
            {
                response.Encoding(cached.ContentEncoding);
            }

            if (cached.ContentLength != null)
            {
                response.Length(cached.ContentLength.Value);
            }

            if (cached.ContentType != null)
            {
                response.Type(cached.ContentType.Value);
            }

            if (cached.Cookies != null)
            {
                foreach (var cookie in cached.Cookies)
                {
                    response.Cookie(cookie.Value);
                }
            }

            if (cached.Expires != null)
            {
                response.Expires(cached.Expires.Value);
            }

            if (cached.Modified != null)
            {
                response.Modified(cached.Modified.Value);
            }

            foreach (var header in cached.Headers)
            {
                response.Header(header.Key, header.Value);
            }

            if (content != null)
            {
                response.Content(new StreamContent(content, () => content.CalculateChecksumAsync()));
            }

            return response.Build();
        }

        private static CachedResponse GetResponse(IResponse response)
        {
            var headers = new Dictionary<string, string>(response.Headers);

            var cookies = new Dictionary<string, Cookie>(response.Cookies);

            List<string>? variations = null;

            if (response.Headers.TryGetValue("Vary", out var header))
            {
                variations = new List<string>();

                foreach (string? entry in header.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    variations.Add(entry);
                }
            }

            return new CachedResponse(response.Status, response.Expires, response.Modified,
                                      variations, headers, cookies, response.ContentType,
                                      response.ContentEncoding, response.ContentLength);
        }

        #endregion

    }

}
