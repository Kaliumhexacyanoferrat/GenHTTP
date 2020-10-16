using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using System;
using System.Collections.Generic;

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

        public IResponse? Handle(IRequest request)
        {
            if (request.HasType(RequestMethod.GET, RequestMethod.HEAD))
            {
                var response = Content.Handle(request);

                if ((response != null) && (response.Content != null))
                {
                    var eTag = CalculateETag(response);

                    var cached = request["If-None-Match"];

                    if ((cached != null) && (cached == eTag))
                    {
                        response.Status = new FlexibleResponseStatus(ResponseStatus.NotModified);
                        
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

                return response;
            }

            return Content.Handle(request);
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => Content.GetContent(request);

        private static string? CalculateETag(IResponse response)
        {
            if (response.Headers.ContainsKey(ETAG_HEADER))
            {
                return response.Headers[ETAG_HEADER];
            }

            if (response.Content != null)
            {
                var checksum = response.Content.Checksum;

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
