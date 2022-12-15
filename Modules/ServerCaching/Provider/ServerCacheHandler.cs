using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Caching;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
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

        private bool Invalidate { get; }

        private Func<IRequest, IResponse, bool>? Predicate { get; }

        #endregion

        #region Initialization

        public ServerCacheHandler(IHandler parent, Func<IHandler, IHandler> contentFactory,
                                  ICache<CachedResponse> meta, ICache<Stream> data,
                                  Func<IRequest, IResponse, bool>? predicate, bool invalidate)
        {
            Parent = parent;
            Content = contentFactory(this);

            Meta = meta;
            Data = data;

            Predicate = predicate;
            Invalidate = invalidate;
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            if (request.HasType(RequestMethod.GET, RequestMethod.HEAD))
            {
                var response = (Invalidate) ? await Content.HandleAsync(request) : null;

                var key = request.GetKey();

                if (TryFindMatching(await Meta.GetEntriesAsync(key), request, out var match))
                {
                    if (match != null)
                    {
                        if (!Invalidate || !await HasChanged(response!, match))
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

                if ((response != null) && (response.Status == ResponseStatus.OK || response.Status == ResponseStatus.NoContent))
                {
                    if ((Predicate == null) || Predicate(request, response))
                    {
                        var cached = await GetResponse(response, request);

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
                }

                return response;
            }

            return await Content.HandleAsync(request);
        }

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

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
                        if (request.Headers.TryGetValue(header.Key, out var actual))
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

            return (response != null);
        }

        private static IResponse GetResponse(IRequest request, CachedResponse cached, Stream? content)
        {
            var response = request.Respond()
                                  .Status(cached.StatusCode, cached.StatusPhrase);

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
                response.Type(cached.ContentType);
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
                response.Content(new StreamContent(content, cached.ContentLength, () => new ValueTask<ulong?>(cached.ContentChecksum)));
            }

            return response.Build();
        }

        private async static ValueTask<CachedResponse> GetResponse(IResponse response, IRequest request)
        {
            var headers = new Dictionary<string, string>(response.Headers);

            var cookies = new Dictionary<string, Cookie>(response.Cookies);

            Dictionary<string, string>? variations = null;

            if (response.Headers.TryGetValue("Vary", out var header))
            {
                variations = new();

                foreach (string? entry in header.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (request.Headers.TryGetValue(entry, out var value))
                    {
                        variations.Add(entry, value);
                    }
                }
            }

            ulong? checksum = null;

            if (response.Content != null)
            {
                checksum = await response.Content.CalculateChecksumAsync();
            }

            return new CachedResponse(response.Status.RawStatus, response.Status.Phrase, response.Expires, response.Modified,
                                      variations, headers, cookies, response.ContentType?.RawType,
                                      response.ContentEncoding, response.ContentLength,
                                      checksum);
        }

        private async static ValueTask<bool> HasChanged(IResponse current, CachedResponse cached)
        {
            var checksum = (current.Content != null) ? await current.Content.CalculateChecksumAsync() : null;

            return (cached.ContentChecksum != checksum)
                || (cached.ContentEncoding != current.ContentEncoding)
                || (cached.ContentLength != current.ContentLength)
                || (cached.ContentType != current.ContentType?.RawType)
                || (cached.Expires != current.Expires)
                || (cached.Modified != current.Modified)
                || (current.Headers.Count != cached.Headers.Count || current.Headers.Except(cached.Headers).Any())
                || (current.Cookies.Count != cached.Cookies?.Count || current.Cookies.Except(cached.Cookies).Any());
        }

        #endregion

    }

}
