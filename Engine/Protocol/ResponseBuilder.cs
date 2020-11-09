using System;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Protocol
{

    internal class ResponseBuilder : IResponseBuilder
    {
        private readonly Response _Response = new Response();

        #region Get-/Setters

        public IRequest Request { get; }

        #endregion

        #region Initialization

        public ResponseBuilder(IRequest request)
        {
            Request = request;
        }

        #endregion

        #region Functionality

        public IResponseBuilder Length(ulong length)
        {
            _Response.ContentLength = length;
            return this;
        }

        public IResponseBuilder Content(IResponseContent content)
        {
            _Response.Content = content;
            _Response.ContentLength = content.Length;

            return this;
        }

        public IResponseBuilder Type(FlexibleContentType contentType)
        {
            _Response.ContentType = contentType;
            return this;
        }

        public IResponseBuilder Cookie(Cookie cookie)
        {
            _Response.WriteableCookies[cookie.Name] = cookie;
            return this;
        }

        public IResponseBuilder Header(string key, string value)
        {
            _Response.Headers.Add(key, value);
            return this;
        }

        public IResponseBuilder Encoding(string encoding)
        {
            _Response.ContentEncoding = encoding;
            return this;
        }

        public IResponseBuilder Expires(DateTime expiryDate)
        {
            _Response.Expires = expiryDate;
            return this;
        }

        public IResponseBuilder Modified(DateTime modificationDate)
        {
            _Response.Modified = modificationDate;
            return this;
        }

        public IResponseBuilder Status(ResponseStatus status)
        {
            _Response.Status = new FlexibleResponseStatus(status);
            return this;
        }

        public IResponseBuilder Status(int status, string reason)
        {
            _Response.Status = new FlexibleResponseStatus(status, reason);
            return this;
        }

        public IResponse Build() => _Response;

        #endregion

    }

}
