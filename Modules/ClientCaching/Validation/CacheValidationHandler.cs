using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.ClientCaching.Validation
{

    public sealed class CacheValidationHandler : IConcern
    {
        private const string ETAG_HEADER = "ETag";

        #region Get-/Setters

        public IHandler Parent { get; }

        public IHandler Content { get; }

        #endregion

        #region Initialization

        public CacheValidationHandler(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            Parent = parent;
            Content = contentFactory(this);
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var response = await Content.HandleAsync(request).ConfigureAwait(false);

            if (request.HasType(RequestMethod.GET, RequestMethod.HEAD))
            {
                if ((response is not null) && (response.Content is not null))
                {
                    var eTag = await CalculateETag(response);

                    var cached = request["If-None-Match"];

                    if ((cached is not null) && (cached == eTag))
                    {
                        response.Status = new(ResponseStatus.NotModified);

                        response.Content = null;

                        response.ContentEncoding = null;
                        response.ContentLength = null;
                        response.ContentType = null;
                    }

                    if (eTag is not null)
                    {
                        response.Headers[ETAG_HEADER] = eTag;
                    }
                }
            }

            return response;
        }

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        private static async ValueTask<string?> CalculateETag(IResponse response)
        {
            if (response.Headers.TryGetValue(ETAG_HEADER, out var eTag))
            {
                return eTag;
            }

            if (response.Content is not null)
            {
                var checksum = await response.Content.CalculateChecksumAsync().ConfigureAwait(false);

                if (checksum is not null)
                {
                    return $"\"{checksum}\"";
                }
            }

            return null;
        }

        #endregion

    }

}
