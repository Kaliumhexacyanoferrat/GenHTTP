using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Core.Protocol
{

    internal class ResponseBuilder : IResponseBuilder
    {
        protected FlexibleResponseStatus? _Status;

        protected Stream? _Content;
        protected ulong? _ContentLength;
        protected FlexibleContentType? _ContentType;
        protected string? _ContentEncoding;

        protected DateTime? _Expires;
        protected DateTime? _Modified;

        protected CookieCollection _Cookies = new CookieCollection();
        protected HeaderCollection _Headers = new HeaderCollection();

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

        public IResponseBuilder Content(Stream body, ContentType contentType)
        {
            return Content(body, new FlexibleContentType(contentType));
        }

        public IResponseBuilder Content(Stream body, FlexibleContentType contentType)
        {
            _Content = body;
            _ContentType = contentType;

            if (body.Length > 0)
            {
                _ContentLength = (ulong)body.Length;
            }

            return this;
        }

        public IResponseBuilder Content(Stream body, ulong length, ContentType contentType)
        {
            return Content(body, length, new FlexibleContentType(contentType));
        }

        public IResponseBuilder Content(Stream body, ulong length, FlexibleContentType contentType)
        {
            _ContentLength = length;
            _Content = body;
            _ContentType = contentType;

            return this;
        }

        public IResponseBuilder Cookie(Cookie cookie)
        {
            _Cookies[cookie.Name] = cookie;
            return this;
        }

        public IResponseBuilder Header(string key, string value)
        {
            _Headers[key] = value;
            return this;
        }

        public IResponseBuilder Encoding(string encoding)
        {
            _ContentEncoding = encoding;
            return this;
        }

        public IResponseBuilder Expires(DateTime expiryDate)
        {
            _Expires = expiryDate;
            return this;
        }

        public IResponseBuilder Modified(DateTime modificationDate)
        {
            _Modified = modificationDate;
            return this;
        }

        public IResponseBuilder Type(ResponseStatus status)
        {
            _Status = new FlexibleResponseStatus(status);
            return this;
        }

        public IResponseBuilder Type(int status, string phrase)
        {
            _Status = new FlexibleResponseStatus(status, phrase);
            return this;
        }

        public IResponse Build()
        {
            if (_Status == null)
            {
                throw new BuilderMissingPropertyException("Type");
            }

            return new Response(_Status, _Headers, _Cookies)
            {
                Content = _Content,
                ContentEncoding = _ContentEncoding,
                ContentLength = _ContentLength,
                ContentType = _ContentType,
                Expires = _Expires,
                Modified = _Modified
            };
        }

        #endregion

    }

}
