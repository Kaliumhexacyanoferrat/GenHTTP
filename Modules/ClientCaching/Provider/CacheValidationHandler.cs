using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

using PooledAwait;

namespace GenHTTP.Modules.ClientCaching.Provider
{

    public class CacheValidationHandler : IConcern
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
                if ((response != null) && (response.Content != null))
                {
                    var eTag = await CalculateETag(response);

                    var cached = request["If-None-Match"];

                    if ((cached != null) && (cached == eTag))
                    {
                        response.Status = new(ResponseStatus.NotModified);

                        response.Content = null;

                        response.ContentEncoding = null;
                        response.ContentLength = null;
                        response.ContentType = null;
                    }

                    if (eTag != null)
                    {
                        response.Headers[ETAG_HEADER] = eTag;
                    }
                }
            }

            return response;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => Content.GetContent(request);

        private static async PooledValueTask<string?> CalculateETag(IResponse response)
        {
            if (response.Headers.TryGetValue(ETAG_HEADER, out var eTag))
            {
                return eTag;
            }

            if (response.Content != null)
            {
                var checksum = await response.Content.CalculateChecksumAsync().ConfigureAwait(false);

                if (checksum != null)
                {
                    return $"\"{checksum}\"";
                }
            }

            return null;
        }

        #endregion

    }

}
